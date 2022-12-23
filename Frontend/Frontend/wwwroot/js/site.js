// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


const robotList = document.getElementById('robot-list');
const locationList = document.getElementById('location-list');
const addRobotButton = document.getElementById('add-robot-button');
const addLocationButton = document.getElementById('add-location-button');
const editModal = document.getElementById('edit-modal');
const editForm = document.getElementById('edit-form');
const editModalTitle = document.getElementById('edit-modal-title');
const editIdInput = document.getElementById('edit-id-input');
const editNameInput = document.getElementById('edit-name-input');
const editTypeInputContainer = document.getElementById('edit-type-input-container');
const editTypeInput = document.getElementById('edit-type-input');
const editLocationInputContainer = document.getElementById('edit-location-input-container');
const editLocationInput = document.getElementById('edit-location-input');
const editLatitudeInputContainer = document.getElementById('edit-latitude-input-container');
const editLatitudeInput = document.getElementById('edit-latitude-input');
const editLongitudeInputContainer = document.getElementById('edit-longitude-input-container');
const editLongitudeInput = document.getElementById('edit-longitude-input');
const cancelEditButton = document.getElementById('cancel-edit-button');

let robots;
let locations;

// Fetch the list of robots and locations from the backend and display them in the tables
fetchRobotsAndLocations();

// Add event listeners for the Add Robot and Add Location buttons
addRobotButton.addEventListener('click', () => showEditModal('add', 'robot'));
addLocationButton.addEventListener('click', () => showEditModal('add', 'location'));

// Add event listener for the Edit form submit button
editForm.addEventListener('submit', (event) => {
    event.preventDefault();
    const action = editForm.dataset.action;
    const type = editForm.dataset.type;
    if (action === 'add') {
        addItem(type);
    } else if (action === 'edit') {
        editItem(type);
    }
});

// Add event listener for the Cancel Edit button
cancelEditButton.addEventListener('click', () => hideEditModal());

// Fetch the list of robots and locations from the backend and display them in the tables
function fetchRobotsAndLocations() {
    // Send RPC request to fetch the list of robots
    RobotClient.getRobots({}, (error, response) => {
        if (error) {
            console.error(error);
            return;
        }
        robots = response.robots;
        updateRobotList();
    });

    // Send RPC request to fetch the list of locations
    LocationClient.getLocations({}, (error, response) => {
        if (error) {
            console.error(error);
            return;
        }
        locations = response.locations;
        updateLocationList();
    });
}

// Update the list of robots in the table
function updateRobotList() {
    // Clear the current list of robots
    robotList.innerHTML = '';

    // Add a table row for each robot
    robots.forEach((robot) => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${robot.id}</td
<td>${robot.name}</td>
            <td>${robot.type}</td>
            <td>${robot.location.name}</td>
            <td>
                <button data-id="${robot.id}" class="edit-button">Edit</button>
                <button data-id="${robot.id}" class="delete-button">Delete</button>
            </td>
        `;
        robotList.appendChild(row);
    });

    // Add event listeners for the Edit and Delete buttons
    const editButtons = document.querySelectorAll('.edit-button');
    editButtons.forEach((button) => button.addEventListener('click', () => showEditModal('edit', 'robot', button.dataset.id)));
    const deleteButtons = document.querySelectorAll('.delete-button');
    deleteButtons.forEach((button) => button.addEventListener('click', () => deleteItem('robot', button.dataset.id)));
}

// Update the list of locations in the table
function updateLocationList() {
    // Clear the current list of locations
    locationList.innerHTML = '';

    // Add a table row for each location
    locations.forEach((location) => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${location.id}</td>
            <td>${location.name}</td>
            <td>${location.latitude}</td>
            <td>${location.longitude}</td>
            <td>
                <button data-id="${location.id}" class="edit-button">Edit</button>
                <button data-id="${location.id}" class="delete-button">Delete</button>
            </td>
        `;
        locationList.appendChild(row);
    });

    // Add event listeners for the Edit and Delete buttons
    const editButtons = document.querySelectorAll('.edit-button');
    editButtons.forEach((button) => button.addEventListener('click', () => showEditModal('edit', 'location', button.dataset.id)));
    const deleteButtons = document.querySelectorAll('.delete-button');
    deleteButtons.forEach((button) => button.addEventListener('click', () => deleteItem('location', button.dataset.id)));
}

// Show the Edit modal for adding or editing a robot or location
function showEditModal(action, type, id) {
    // Set the action and type in the form data
    editForm.dataset.action = action;
    editForm.dataset.type = type;

    // Update the modal title and form inputs based on the action and type
    if (action === 'add') {
        editModalTitle.textContent = `Add ${type === 'robot' ? 'Robot' : 'Location'}`;
        editIdInput.value = '';
        editNameInput.value = '';
        if (type === 'robot') {
            editTypeInputContainer.style.display = 'block';
            editTypeInput.value = '';
            editLocationInputContainer.style.display = 'block';
            editLocationInput.innerHTML = '';
            locations.forEach((location) => {
                const option = document.createElement('option');
                option.value = location.id;
                option.textContent = location.name;
                editLocationInput.appendChild(option);
            });
            editLatitudeInputContainer.style.display = 'none';
            editLatitudeInput.value = '';
            editLongitudeInputContainer.style.display = 'none';
            editLongitudeInput.value = '';
        } else if (type === 'location') {
            editTypeInputContainer.style.display = 'none';
            editTypeInput.value = '';
            editLocationInputContainer.style.display = 'none';
            editLocationInput.innerHTML = '';
            editLatitudeInputContainer.style.display = 'block';
            editLatitudeInput.value = '';
            editLongitudeInputContainer.style.display = 'block';
            editLongitudeInput.value = '';
        }
    } else if (action === 'edit') {
        editModalTitle.textContent = `Edit ${type === 'robot' ? 'Robot' : 'Location'}`;
        const item = type === 'robot' ? robots.find((robot) => robot.id === id) : locations.find((location) => location.id === id);
        editIdInput.value = item.id;
        editNameInput.value = item.name;
        if (type === 'robot') {
            editTypeInputContainer.style.display = 'block';
            editTypeInput.value = item.type;
            editLocationInputContainer.style.display = 'block';
            editLocationInput.innerHTML = '';
            locations.forEach((location) => {
                const option = document.createElement('option');
                option.value = location.id;
                option.textContent = location.name;
                if (location.id === item.location.id) {
                    option.selected = true;
                }
                editLocationInput.appendChild(option);
            });
            editLatitudeInputContainer.style.display = 'none';
            editLatitudeInput.value = '';
            editLongitudeInputContainer.style.display = 'none';
            editLongitudeInput.value = '';
        } else if (type === 'location') {
            editTypeInputContainer.style.display = 'none';
            editTypeInput.value = '';
            editLocationInputContainer.style.display = 'none';
            editLocationInput.innerHTML = '';
            editLatitudeInputContainer.style.display = 'block';
            editLatitudeInput.value = item.latitude;
            editLongitudeInputContainer.style.display = 'block';
            editLongitudeInput.value = item.longitude;
        }
    }

    // Show the modal
    editModal.style.display = 'block';
}

// Hide the Edit modal
function hideEditModal() {
    // Reset the form data and inputs
    editForm.dataset.action = '';
    editForm.dataset.type = '';
    editModalTitle.textContent = '';
    editIdInput.value = '';
    editNameInput.value = '';
    editTypeInputContainer.style.display = 'none';
    editTypeInput.value = '';
    editLocationInputContainer.style.display = 'none';
    editLocationInput.innerHTML = '';
    editLatitudeInputContainer.style.display = 'none';
    editLatitudeInput.value = '';
    editLongitudeInputContainer.style.display = 'none';
    editLongitudeInput.value = '';

    // Hide the modal
    editModal.style.display = 'none';
}

// Add a new robot or location
function addItem(type) {
    const request = {};
    if (type === 'robot') {
        request.robot = {
            name: editNameInput.value,
            type: editTypeInput.value,
            location: {
                id: editLocationInput.value,
            },
        };
        RobotClient.addRobot(request, (error, response) => {
            if (error) {
                console.error(error);
                return;
            }
            robots.push(response.robot);
            updateRobotList();
            hideEditModal();
        });
    } else if (type === 'location') {
        request.location = {
            name: editNameInput.value,
            latitude: parseFloat(editLatitudeInput.value),
            longitude: parseFloat(editLongitudeInput.value),
        };
        LocationClient.addLocation(request, (error, response) => {
            if (error) {
                console.error(error);
                return;
            }
            locations.push(response.location);
            updateLocationList();
            hideEditModal();
        });
    }
}

// Edit an existing robot or location
function editItem(type) {
    const request = {};
    if (type === 'robot') {
        request.robot = {
            id: editIdInput.value,
            name: editNameInput.value,
            type: editTypeInput.value,
            location: {
                id: editLocationInput.value,
            },
        };
        RobotClient.updateRobot(request, (error, response) => {
            if (error) {
                console.error(error);
                return;
            }
            const index = robots.findIndex((robot) => robot.id === response.robot.id);
            robots[index] = response.robot;
            updateRobotList();
            hideEditModal();
        });
    } else if (type === 'location') {
        request.location = {
            id: editIdInput.value,
            name: editNameInput.value,
            latitude: parseFloat(editLatitudeInput.value),
            longitude: parseFloat(editLongitudeInput.value),
        };
        LocationClient.updateLocation(request, (error, response) => {
            if (error) {
                console.error(error);
                return;
            }
            const index = locations.findIndex((location) => location.id === response.location.id);
            locations[index] = response.location;
            updateLocationList();
            hideEditModal();
        });
    }
}

// Delete a robot or location
function deleteItem(type, id) {
    if (type === 'robot') {
        RobotClient.deleteRobot({ id }, (error) => {
            if (error) {
                console.error(error);
                return;
            }
            const index = robots.findIndex((robot) => robot.id === id);
            robots.splice(index, 1);
            updateRobotList();
        });
    } else if (type === 'location') {
        LocationClient.deleteLocation({ id }, (error) => {
            if (error) {
                console.error(error);
                return;
            }
            const index = locations.findIndex((location) => location.id === id);
            locations.splice(index, 1);
            updateLocationList();
        });
    }
}

// Get the list of robots and locations from the server
function getRobotsAndLocations() {
    RobotClient.listRobots({}, (error, response) => {
        if (error) {
            console.error(error);
            return;
        }
        robots = response.robots;
        updateRobotList();
    });
    LocationClient.listLocations({}, (error, response) => {
        if (error) {
            console.error(error);
            return;
        }
        locations = response.locations;
        updateLocationList();
    });
}

// Initialize the app
function init() {
    // Load the robots and locations
    getRobotsAndLocations();

    // Add event listeners for the Add buttons
    addRobotButton.addEventListener('click', () => showEditModal('add', 'robot'));
    addLocationButton.addEventListener('click', () => showEditModal('add', 'location'));

    // Add event listeners for the Edit modal
    editForm.addEventListener('submit', (event) => {
        event.preventDefault();
        if (editForm.dataset.action === 'add') {
            addItem(editForm.dataset.type);
        } else if (editForm.dataset.action === 'edit') {
            editItem(editForm.dataset.type);
        }
    });
    cancelEditButton.addEventListener('click', hideEditModal);
}

init();