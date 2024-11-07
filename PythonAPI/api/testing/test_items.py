# test_items.py
import pytest
import httpx
import json
from datetime import datetime

# Expliciete mock data voor items
MOCK_ITEMS = {
    "1": {
        "item_id": "1",
        "name": "Laptop",
        "description": "High-end laptop",
        "category": "Electronics",
        "price": 999.99,
        "sku": "LAP001",
        "created_at": "2024-01-01T00:00:00",
        "updated_at": "2024-01-01T00:00:00"
    },
    "2": {
        "item_id": "2",
        "name": "Office Chair",
        "description": "Ergonomic chair",
        "category": "Furniture",
        "price": 199.99,
        "sku": "CHR001",
        "created_at": "2024-01-02T00:00:00",
        "updated_at": "2024-01-02T00:00:00"
    }
}

class MockItemTable:
    def __init__(self):
        self.items = MOCK_ITEMS.copy()
        
    def create_item(self, item_data):
        item_id = item_data.get('item_id')
        self.items[item_id] = {
            **item_data,
            'created_at': datetime.now().isoformat(),
            'updated_at': datetime.now().isoformat()
        }
        return self.items[item_id]
    
    def get_item(self, item_id):
        return self.items.get(item_id)
    
    def list_items(self):
        return list(self.items.values())
    
    def update_item(self, item_id, item_data):
        if item_id in self.items:
            self.items[item_id] = {
                **self.items[item_id],
                **item_data,
                'updated_at': datetime.now().isoformat()
            }
            return self.items[item_id]
        return None
    
    def delete_item(self, item_id):
        return self.items.pop(item_id, None) is not None

@pytest.fixture
def mock_db():
    return MockItemTable()

@pytest.fixture
def client(mock_db):
    headers = {
        "API_KEY": "test_api_key",
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
        
        if path == "/api/v1/items/" and method == "POST":
            data = json.loads(request.content)
            item = mock_db.create_item(data)
            return httpx.Response(201, json=item)
            
        elif path.startswith("/api/v1/items/"):
            item_id = path.split("/")[-1]
            if method == "GET":
                if path == "/api/v1/items/":
                    items = mock_db.list_items()
                    return httpx.Response(200, json=items)
                item = mock_db.get_item(item_id)
                if item:
                    return httpx.Response(200, json=item)
                return httpx.Response(404, json={"error": "Item not found"})
                
            elif method == "PUT":
                data = json.loads(request.content)
                item = mock_db.update_item(item_id, data)
                if item:
                    return httpx.Response(200, json=item)
                return httpx.Response(404, json={"error": "Item not found"})
                
            elif method == "DELETE":
                if mock_db.delete_item(item_id):
                    return httpx.Response(200, json={"message": "Item deleted"})
                return httpx.Response(404, json={"error": "Item not found"})
                
        return httpx.Response(404)
        
    client._transport = httpx.MockTransport(mock_response)
    return client

def test_list_items(client):
    """Test: Get all items"""
    response = client.get("/api/v1/items/")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == len(MOCK_ITEMS)
    assert any(item["name"] == "Laptop" for item in data)

def test_get_item(client):
    """Test: Get specific item"""
    response = client.get("/api/v1/items/1")
    assert response.status_code == 200
    data = response.json()
    assert data["name"] == "Laptop"
    assert data["price"] == 999.99

def test_create_item(client):
    """Test: Create new item"""
    new_item = {
        "item_id": "3",
        "name": "Monitor",
        "description": "4K Display",
        "category": "Electronics",
        "price": 299.99,
        "sku": "MON001"
    }
    response = client.post("/api/v1/items/", json=new_item)
    assert response.status_code == 201
    data = response.json()
    assert data["name"] == "Monitor"
    assert "created_at" in data

def test_update_item(client):
    """Test: Update existing item"""
    update_data = {
        "price": 899.99,
        "description": "Updated laptop description"
    }
    response = client.put("/api/v1/items/1", json=update_data)
    assert response.status_code == 200
    data = response.json()
    assert data["price"] == 899.99
    assert data["description"] == "Updated laptop description"

def test_delete_item(client):
    """Test: Delete item"""
    response = client.delete("/api/v1/items/1")
    assert response.status_code == 200
    
    # Verify deletion
    get_response = client.get("/api/v1/items/1")
    assert get_response.status_code == 404