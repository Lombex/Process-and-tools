import pytest
import httpx
from datetime import datetime
import uuid

@pytest.fixture
def client():
    """Fixture to create a real HTTP client with authentication."""
    headers = {
        "API_KEY": "a1b2c3d4e5",
        "Content-Type": "application/json"
    }
    return httpx.Client(
        base_url="http://localhost:3000/api/v1",
        headers=headers,
        timeout=30.0
    )

@pytest.fixture
def test_supplier_data():
    """Fixture to generate unique test supplier data."""
    test_id = f"TEST-{uuid.uuid4().hex[:8]}"
    return {
        "supplier_id": test_id,
        "name": f"Test Supplier {test_id}",
        "contact_name": "Test Contact",
        "email": f"test_{test_id}@example.com",
        "phone": "123-456-7890",
        "address": "Test Address",
        "test_flag": True  # To identify test data
    }

@pytest.fixture
def created_supplier(client, test_supplier_data):
    """Fixture to create and cleanup a test supplier."""
    # Create the supplier
    response = client.post("/suppliers/", json=test_supplier_data)
    assert response.status_code == 201
    created = response.json()
    
    # Return the created supplier for test use
    yield created
    
    # Cleanup after test
    try:
        client.delete(f"/suppliers/{created['supplier_id']}")
    except Exception as e:
        print(f"Cleanup warning for supplier {created['supplier_id']}: {e}")

def test_list_suppliers(client, created_supplier):
    """Test getting all suppliers while ensuring our test supplier is included."""
    response = client.get("/suppliers/")
    assert response.status_code == 200
    suppliers = response.json()
    
    assert isinstance(suppliers, list)
    test_supplier_found = any(
        supplier['supplier_id'] == created_supplier['supplier_id'] 
        for supplier in suppliers
    )
    assert test_supplier_found, "Test supplier not found in list"

def test_get_supplier(client, created_supplier):
    """Test getting a specific supplier."""
    response = client.get(f"/suppliers/{created_supplier['supplier_id']}")
    assert response.status_code == 200
    
    supplier = response.json()
    assert supplier['supplier_id'] == created_supplier['supplier_id']
    assert supplier['name'] == created_supplier['name']

def test_create_supplier(client, test_supplier_data):
    """Test creating a new supplier."""
    response = client.post("/suppliers/", json=test_supplier_data)
    assert response.status_code == 201
    
    created = response.json()
    assert created['name'] == test_supplier_data['name']
    assert created['email'] == test_supplier_data['email']
    
    # Cleanup
    client.delete(f"/suppliers/{created['supplier_id']}")

def test_update_supplier(client, created_supplier):
    """Test updating an existing supplier."""
    update_data = {
        "email": f"updated_{created_supplier['supplier_id']}@example.com",
        "phone": "999-999-9999"
    }
    
    response = client.put(
        f"/suppliers/{created_supplier['supplier_id']}", 
        json=update_data
    )
    assert response.status_code == 200
    
    updated = response.json()
    assert updated['email'] == update_data['email']
    assert updated['phone'] == update_data['phone']
    # Original fields should remain unchanged
    assert updated['name'] == created_supplier['name']

def test_delete_supplier(client, test_supplier_data):
    """Test deleting a supplier."""
    # First create a supplier to delete
    create_response = client.post("/suppliers/", json=test_supplier_data)
    assert create_response.status_code == 201
    created = create_response.json()
    
    # Delete the supplier
    delete_response = client.delete(f"/suppliers/{created['supplier_id']}")
    assert delete_response.status_code == 200
    
    # Verify it's gone
    get_response = client.get(f"/suppliers/{created['supplier_id']}")
    assert get_response.status_code == 404

def test_get_nonexistent_supplier(client):
    """Test getting a supplier that doesn't exist."""
    nonexistent_id = f"NONEXISTENT-{uuid.uuid4().hex[:8]}"
    response = client.get(f"/suppliers/{nonexistent_id}")
    assert response.status_code == 404

def test_create_invalid_supplier(client):
    """Test creating a supplier with invalid data."""
    invalid_data = {
        "supplier_id": "",  # Invalid empty ID
        "name": "",        # Invalid empty name
        "email": "not-an-email"  # Invalid email format
    }
    
    response = client.post("/suppliers/", json=invalid_data)
    assert response.status_code == 400
