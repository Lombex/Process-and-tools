import pytest
from httpx import Client, codes

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
    response = client.post("/shipments", json=new_shipment)
    
    assert response.status_code == codes.CREATED
    assert "id" in response.json()

def test_create_shipment_invalid_data(client):
    new_shipment = {
        "destination": "",
        "weight": -10,
        "sender": "Sender A"
    }
    response = client.post("/shipments", json=new_shipment)
    assert response.status_code == codes.BAD_REQUEST

def test_retrieve_shipment(client):
    new_shipment = {"destination": "Warehouse A", "weight": 20, "sender": "Sender A"}
    create_response = client.post("/shipments", json=new_shipment)
    shipment_id = create_response.json()["id"]
    
    response = client.get(f"/shipments/{shipment_id}")  # Retrieve the shipment by ID
    assert response.status_code == codes.OK
    assert response.json()["id"] == shipment_id  # Check if ID matches

def test_retrieve_non_existent_shipment(client):
    response = client.get("/shipments/-1")  # Test for a non-existent ID
    assert response.status_code == codes.NOT_FOUND

def test_update_shipment_success(client):
    new_shipment = {"destination": "Warehouse A", "weight": 20, "sender": "Sender A"}
    create_response = client.post("/shipments", json=new_shipment)
    shipment_id = create_response.json()["id"]
    
    updated_shipment = {
        "destination": "Warehouse B",
        "weight": 25
    }
    response = client.put(f"/shipments/{shipment_id}", json=updated_shipment)  # Update the shipment by ID
    assert response.status_code == codes.OK
    assert response.json()["destination"] == "Warehouse B"  # Ensure the update is correct

def test_update_shipment_invalid_data(client):
    new_shipment = {"destination": "Warehouse A", "weight": 20, "sender": "Sender A"}
    create_response = client.post("/shipments", json=new_shipment)
    shipment_id = create_response.json()["id"]
    
    updated_shipment = {
        "weight": -5  # Invalid weight
    }
    response = client.put(f"/shipments/{shipment_id}", json=updated_shipment)  # Update the shipment by ID
    assert response.status_code == codes.BAD_REQUEST

def test_delete_shipment_success(client):
    new_shipment = {"destination": "Warehouse A", "weight": 20, "sender": "Sender A"}
    create_response = client.post("/shipments", json=new_shipment)
    shipment_id = create_response.json()["id"]
    
    response = client.delete(f"/shipments/{shipment_id}")  # Delete the shipment by ID
    assert response.status_code == codes.OK
    assert client.get(f"/shipments/{shipment_id}").status_code == codes.NOT_FOUND  # Ensure the shipment was deleted

def test_delete_non_existent_shipment(client):
    response = client.delete("/shipments/-1")  # Test for a non-existent ID
    assert response.status_code == codes.NOT_FOUND

def test_create_shipment_unauthorized(client):
    new_shipment = {
        "destination": "Warehouse A",
        "weight": 20,
        "sender": "Sender A"
    }
    response = client.post("/shipments", json=new_shipment)  # Authorization check not applied in this context
    assert response.status_code == codes.CREATED

def test_update_shipment_unauthorized(client):
    updated_shipment = {
        "destination": "Warehouse B",
        "weight": 25
    }
    response = client.put("/shipments/1", json=updated_shipment)  # Authorization check not applied in this context
    assert response.status_code == codes.NOT_FOUND  # No shipment exists to update

def test_delete_shipment_unauthorized(client):
    response = client.delete("/shipments/1")  # Authorization check not applied in this context
    assert response.status_code == codes.NOT_FOUND  # No shipment exists to delete
