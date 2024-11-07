import pytest
import httpx
import json
from datetime import datetime

# Mock database
class MockInventoryTable:
    def __init__(self):
        self.inventories = {}
        
    def create_inventory(self, inventory_data):
        inventory_id = inventory_data.get('inventory_id')
        self.inventories[inventory_id] = {
            **inventory_data,
            'created_at': datetime.now().isoformat(),
            'updated_at': datetime.now().isoformat()
        }
        return self.inventories[inventory_id]
    
    def get_inventory(self, inventory_id):
        return self.inventories.get(inventory_id)
    
    def list_inventories(self):
        return list(self.inventories.values())
    
    def update_inventory(self, inventory_id, inventory_data):
        if inventory_id in self.inventories:
            self.inventories[inventory_id] = {
                **self.inventories[inventory_id],
                **inventory_data,
                'updated_at': datetime.now().isoformat()
            }
            return self.inventories[inventory_id]
        return None
    
    def delete_inventory(self, inventory_id):
        if inventory_id in self.inventories:
            del self.inventories[inventory_id]
            return True
        return False

@pytest.fixture
def mock_db():
    return MockInventoryTable()

@pytest.fixture
def client(mock_db):
    """Create a mocked HTTP client"""
    headers = {
        "API_KEY": "a1b2c3d4e5",
        "Content-Type": "application/json"
    }
    
    client = httpx.Client(
        base_url="http://localhost:3000",
        headers=headers,
        timeout=30.0
    )
    
    def mock_response(request):
        path = request.url.path
        method = request.method
        
        if path == "/api/v1/inventories/" and method == "POST":
            data = json.loads(request.content)
            inventory = mock_db.create_inventory(data)
            return httpx.Response(201, json=inventory)
            
        elif path.startswith("/api/v1/inventories/") and method == "GET":
            if path == "/api/v1/inventories/":
                inventories = mock_db.list_inventories()
                return httpx.Response(200, json=inventories)
            else:
                inventory_id = path.split("/")[-1]
                inventory = mock_db.get_inventory(inventory_id)
                if inventory:
                    return httpx.Response(200, json=inventory)
                return httpx.Response(404, json={"error": "Inventory not found"})
                
        elif path.startswith("/api/v1/inventories/") and method == "PUT":
            inventory_id = path.split("/")[-1]
            data = json.loads(request.content)
            inventory = mock_db.update_inventory(inventory_id, data)
            if inventory:
                return httpx.Response(200, json=inventory)
            return httpx.Response(404, json={"error": "Inventory not found"})
                
        elif path.startswith("/api/v1/inventories/") and method == "DELETE":
            inventory_id = path.split("/")[-1]
            success = mock_db.delete_inventory(inventory_id)
            if success:
                return httpx.Response(200, json={"message": "Inventory deleted"})
            return httpx.Response(404, json={"error": "Inventory not found"})
            
        return httpx.Response(404)
        
    client._transport = httpx.MockTransport(mock_response)
    return client

@pytest.fixture
def sample_inventory():
    """Test inventory data"""
    return {
        "inventory_id": "1",
        "name": "Sample Inventory",
        "total_on_hand": 50,
        "location_id": "WH-A",
        "total_ordered": 0,
        "total_allocated": 0,
        "total_available": 50,
        "total_expected": 50
    }

def test_create_inventory(client, sample_inventory):
    """Test: Create inventory"""
    response = client.post("/api/v1/inventories/", json=sample_inventory)
    assert response.status_code == 201
    
    data = response.json()
    assert data["inventory_id"] == sample_inventory["inventory_id"]
    assert data["name"] == sample_inventory["name"]
    assert "created_at" in data
    assert "updated_at" in data

def test_get_inventory(client, sample_inventory):
    """Test: Get inventory"""
    client.post("/api/v1/inventories/", json=sample_inventory)
    
    response = client.get(f"/api/v1/inventories/{sample_inventory['inventory_id']}")
    assert response.status_code == 200
    
    data = response.json()
    assert data["inventory_id"] == sample_inventory["inventory_id"]
    assert data["name"] == sample_inventory["name"]

def test_get_nonexistent_inventory(client):
    """Test: Get non-existent inventory"""
    response = client.get("/api/v1/inventories/nonexistent")
    assert response.status_code == 404

def test_update_inventory(client, sample_inventory):
    """Test: Update inventory"""
    client.post("/api/v1/inventories/", json=sample_inventory)
    
    update_data = {
        "name": "Updated Inventory",
        "total_on_hand": 100,
        "location_id": "WH-B"
    }
    
    response = client.put(
        f"/api/v1/inventories/{sample_inventory['inventory_id']}", 
        json=update_data
    )
    assert response.status_code == 200
    
    data = response.json()
    assert data["name"] == update_data["name"]
    assert data["total_on_hand"] == update_data["total_on_hand"]
    assert data["location_id"] == update_data["location_id"]

def test_list_inventories(client, sample_inventory):
    """Test: List all inventories"""
    client.post("/api/v1/inventories/", json=sample_inventory)
    
    response = client.get("/api/v1/inventories/")
    assert response.status_code == 200
    
    data = response.json()
    assert isinstance(data, list)
    assert len(data) > 0
    assert any(inv["inventory_id"] == sample_inventory["inventory_id"] for inv in data)

def test_delete_inventory(client, sample_inventory):
    """Test: Delete inventory"""
    client.post("/api/v1/inventories/", json=sample_inventory)
    
    response = client.delete(f"/api/v1/inventories/{sample_inventory['inventory_id']}")
    assert response.status_code == 200
    
    # Verify deletion
    get_response = client.get(f"/api/v1/inventories/{sample_inventory['inventory_id']}")
    assert get_response.status_code == 404