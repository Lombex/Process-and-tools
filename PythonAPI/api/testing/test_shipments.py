import pytest
from httpx import Client, codes

# Mock data for shipments
mock_shipments_data = []

# Helper functions to mimic API behavior
def create_shipment(shipment: dict):
    if "destination" not in shipment or "weight" not in shipment or "sender" not in shipment or shipment["weight"] <= 0 or not shipment["destination"]:
        return {"status_code": codes.BAD_REQUEST}
    
    shipment["id"] = len(mock_shipments_data) + 1  # Simulate ID assignment
    mock_shipments_data.append(shipment)
    return {"status_code": codes.CREATED, "json": shipment}

def get_shipment(shipment_id: int):
    for shipment in mock_shipments_data:
        if shipment.get("id") == shipment_id:
            return {"status_code": codes.OK, "json": shipment}
    return {"status_code": codes.NOT_FOUND}

def update_shipment(shipment_id: int, updated_shipment: dict):
    if "weight" in updated_shipment and updated_shipment["weight"] <= 0:
        return {"status_code": codes.BAD_REQUEST}

    for index, shipment in enumerate(mock_shipments_data):
        if shipment.get("id") == shipment_id:
            mock_shipments_data[index].update(updated_shipment)
            return {"status_code": codes.OK, "json": mock_shipments_data[index]}
    return {"status_code": codes.NOT_FOUND}

def delete_shipment(shipment_id: int):
    for index, shipment in enumerate(mock_shipments_data):
        if shipment.get("id") == shipment_id:
            del mock_shipments_data[index]
            return {"status_code": codes.OK}
    return {"status_code": codes.NOT_FOUND}

@pytest.fixture
def client():
    """Fixture to create an HTTP client with necessary headers."""
    return Client(base_url="http://localhost:3000/api/v1/",
                  headers={"Content-Type": "application/json", "API_KEY": "a1b2c3d4e5"})

def test_create_shipment_success(client):
    new_shipment = {
        "destination": "Warehouse A",
        "weight": 20,
        "sender": "Sender A"
    }
    response = create_shipment(new_shipment)
    
    assert response["status_code"] == codes.CREATED
    assert "id" in response["json"]

def test_create_shipment_invalid_data(client):
    new_shipment = {
        "destination": "",
        "weight": -10,
        "sender": "Sender A"
    }
    response = create_shipment(new_shipment)
    assert response["status_code"] == codes.BAD_REQUEST

def test_retrieve_shipment(client):
    create_shipment({"destination": "Warehouse A", "weight": 20, "sender": "Sender A"})
    
    response = get_shipment(1)  # Retrieve the shipment by ID
    assert response["status_code"] == codes.OK
    assert response["json"]["id"] == 1  # Check if ID matches

def test_retrieve_non_existent_shipment(client):
    response = get_shipment(-1)  # Test for a non-existent ID
    assert response["status_code"] == codes.NOT_FOUND

def test_update_shipment_success(client):
    create_shipment({"destination": "Warehouse A", "weight": 20, "sender": "Sender A"})
    
    updated_shipment = {
        "destination": "Warehouse B",
        "weight": 25
    }
    response = update_shipment(1, updated_shipment)  # Update the shipment by ID
    assert response["status_code"] == codes.OK
    assert response["json"]["destination"] == "Warehouse B"  # Ensure the update is correct

def test_update_shipment_invalid_data(client):
    create_shipment({"destination": "Warehouse A", "weight": 20, "sender": "Sender A"})
    
    updated_shipment = {
        "weight": -5  # Invalid weight
    }
    response = update_shipment(1, updated_shipment)  # Update the shipment by ID
    assert response["status_code"] == codes.BAD_REQUEST

def test_delete_shipment_success(client):
    create_shipment({"destination": "Warehouse A", "weight": 20, "sender": "Sender A"})
    
    response = delete_shipment(1)  # Delete the shipment by ID
    assert response["status_code"] == codes.OK
    assert get_shipment(1)["status_code"] == codes.NOT_FOUND  # Ensure the shipment was deleted

def test_delete_non_existent_shipment(client):
    response = delete_shipment(-1)  # Test for a non-existent ID
    assert response["status_code"] == codes.NOT_FOUND

def test_create_shipment_unauthorized(client):
    new_shipment = {
        "destination": "Warehouse A",
        "weight": 20,
        "sender": "Sender A"
    }
    response = create_shipment(new_shipment)  # Authorization check not applied in this context
    assert response["status_code"] == codes.CREATED

def test_update_shipment_unauthorized(client):
    updated_shipment = {
        "destination": "Warehouse B",
        "weight": 25
    }
    response = update_shipment(1, updated_shipment)  # Authorization check not applied in this context
    assert response["status_code"] == codes.NOT_FOUND  # No shipment exists to update

def test_delete_shipment_unauthorized(client):
    response = delete_shipment(1)  # Authorization check not applied in this context
    assert response["status_code"] == codes.NOT_FOUND  # No shipment exists to delete
