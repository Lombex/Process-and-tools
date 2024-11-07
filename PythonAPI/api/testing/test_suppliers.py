# test_suppliers.py
import pytest
import httpx
import json
from datetime import datetime

# Expliciete mock data voor suppliers
MOCK_SUPPLIERS = {
    "1": {
        "supplier_id": "1",
        "name": "Tech Supplies Inc",
        "contact_name": "John Doe",
        "email": "john@techsupplies.com",
        "phone": "123-456-7890",
        "address": "123 Tech Street",
        "created_at": "2024-01-01T00:00:00",
        "updated_at": "2024-01-01T00:00:00"
    },
    "2": {
        "supplier_id": "2",
        "name": "Office Furniture Co",
        "contact_name": "Jane Smith",
        "email": "jane@officefurniture.com",
        "phone": "098-765-4321",
        "address": "456 Office Road",
        "created_at": "2024-01-02T00:00:00",
        "updated_at": "2024-01-02T00:00:00"
    }
}

class MockSupplierTable:
    def __init__(self):
        self.suppliers = MOCK_SUPPLIERS.copy()
        
    def create_supplier(self, supplier_data):
        supplier_id = supplier_data.get('supplier_id')
        self.suppliers[supplier_id] = {
            **supplier_data,
            'created_at': datetime.now().isoformat(),
            'updated_at': datetime.now().isoformat()
        }
        return self.suppliers[supplier_id]
    
    def get_supplier(self, supplier_id):
        return self.suppliers.get(supplier_id)
    
    def list_suppliers(self):
        return list(self.suppliers.values())
    
    def update_supplier(self, supplier_id, supplier_data):
        if supplier_id in self.suppliers:
            self.suppliers[supplier_id] = {
                **self.suppliers[supplier_id],
                **supplier_data,
                'updated_at': datetime.now().isoformat()
            }
            return self.suppliers[supplier_id]
        return None
    
    def delete_supplier(self, supplier_id):
        return self.suppliers.pop(supplier_id, None) is not None

@pytest.fixture
def mock_db():
    return MockSupplierTable()

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
        
        if path == "/api/v1/suppliers/" and method == "POST":
            data = json.loads(request.content)
            supplier = mock_db.create_supplier(data)
            return httpx.Response(201, json=supplier)
            
        elif path.startswith("/api/v1/suppliers/"):
            supplier_id = path.split("/")[-1]
            if method == "GET":
                if path == "/api/v1/suppliers/":
                    suppliers = mock_db.list_suppliers()
                    return httpx.Response(200, json=suppliers)
                supplier = mock_db.get_supplier(supplier_id)
                if supplier:
                    return httpx.Response(200, json=supplier)
                return httpx.Response(404, json={"error": "Supplier not found"})
                
            elif method == "PUT":
                data = json.loads(request.content)
                supplier = mock_db.update_supplier(supplier_id, data)
                if supplier:
                    return httpx.Response(200, json=supplier)
                return httpx.Response(404, json={"error": "Supplier not found"})
                
            elif method == "DELETE":
                if mock_db.delete_supplier(supplier_id):
                    return httpx.Response(200, json={"message": "Supplier deleted"})
                return httpx.Response(404, json={"error": "Supplier not found"})
                
        return httpx.Response(404)
        
    client._transport = httpx.MockTransport(mock_response)
    return client

def test_list_suppliers(client):
    """Test: Get all suppliers"""
    response = client.get("/api/v1/suppliers/")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == len(MOCK_SUPPLIERS)
    assert any(supplier["name"] == "Tech Supplies Inc" for supplier in data)

def test_get_supplier(client):
    """Test: Get specific supplier"""
    response = client.get("/api/v1/suppliers/1")
    assert response.status_code == 200
    data = response.json()
    assert data["name"] == "Tech Supplies Inc"
    assert data["email"] == "john@techsupplies.com"

def test_create_supplier(client):
    """Test: Create new supplier"""
    new_supplier = {
        "supplier_id": "3",
        "name": "Electronics Wholesale Ltd",
        "contact_name": "Bob Wilson",
        "email": "bob@electronics.com",
        "phone": "555-0123-4567",
        "address": "789 Electronics Ave"
    }
    response = client.post("/api/v1/suppliers/", json=new_supplier)
    assert response.status_code == 201
    data = response.json()
    assert data["name"] == "Electronics Wholesale Ltd"
    assert "created_at" in data

def test_update_supplier(client):
    """Test: Update existing supplier"""
    update_data = {
        "email": "updated@techsupplies.com",
        "phone": "999-888-7777"
    }
    response = client.put("/api/v1/suppliers/1", json=update_data)
    assert response.status_code == 200
    data = response.json()
    assert data["email"] == "updated@techsupplies.com"
    assert data["phone"] == "999-888-7777"

def test_delete_supplier(client):
    """Test: Delete supplier"""
    response = client.delete("/api/v1/suppliers/1")
    assert response.status_code == 200
    
    # Verify deletion
    get_response = client.get("/api/v1/suppliers/1")
    assert get_response.status_code == 404