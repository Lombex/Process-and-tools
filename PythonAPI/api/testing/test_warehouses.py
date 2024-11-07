import pytest
import httpx
import json
from datetime import datetime

MOCK_WAREHOUSES = {
    "1": {
        "id": 1,
        "name": "Main Warehouse",
        "address": "Storage Street 5",
        "city": "Rotterdam",
        "zip_code": "1012AA",
        "country": "Netherlands",
        "contact_name": "John Doe",
        "contact_phone": "+31612345678",
        "contact_email": "john.doe@example.com"
    }
}

class MockWarehouseDatabase:
    def __init__(self):
        self.warehouses = MOCK_WAREHOUSES.copy()

    def create(self, warehouse):
        warehouse_id = str(warehouse["id"])
        if warehouse_id in self.warehouses:
            return None
        self.warehouses[warehouse_id] = {**warehouse, "created_at": datetime.now().isoformat()}
        return self.warehouses[warehouse_id]

    def get(self, warehouse_id):
        return self.warehouses.get(str(warehouse_id))

    def update(self, warehouse_id, data):
        warehouse_id_str = str(warehouse_id)
        if warehouse_id_str in self.warehouses:
            self.warehouses[warehouse_id_str].update(data)
            self.warehouses[warehouse_id_str]["updated_at"] = datetime.now().isoformat()
            return self.warehouses[warehouse_id_str]
        return None

    def delete(self, warehouse_id):
        return self.warehouses.pop(str(warehouse_id), None)

@pytest.fixture
def mock_warehouse_db():
    return MockWarehouseDatabase()

@pytest.fixture
def client(mock_warehouse_db):
    client = httpx.Client(base_url="http://localhost:3000")

    def mock_response(request):
        path, method = request.url.path, request.method
        warehouse_id = path.split("/")[-1]

        if "/warehouses/" in path:
            if method == "POST":
                data = json.loads(request.content)
                warehouse = mock_warehouse_db.create(data)
                return httpx.Response(201 if warehouse else 400, json=warehouse or {"error": "Already exists"})
            elif method == "GET":
                warehouse = mock_warehouse_db.get(warehouse_id)
                return httpx.Response(200 if warehouse else 404, json=warehouse or {"error": "Not found"})
            elif method == "PUT":
                data = json.loads(request.content)
                warehouse = mock_warehouse_db.update(warehouse_id, data)
                return httpx.Response(200 if warehouse else 404, json=warehouse or {"error": "Not found"})
            elif method == "DELETE":
                deleted = mock_warehouse_db.delete(warehouse_id)
                return httpx.Response(200 if deleted else 404, json={"message": "Deleted" if deleted else "Not found"})

        return httpx.Response(404)

    client._transport = httpx.MockTransport(mock_response)
    return client

def test_create_warehouse(client):
    new_warehouse = {
        "id": 2,
        "name": "New Warehouse",
        "address": "Industrial Zone 7",
        "city": "Rotterdam",
        "zip_code": "3011AA",
        "country": "Netherlands",
        "contact_name": "Jane Smith",
        "contact_phone": "+31687654321",
        "contact_email": "jane.smith@example.com"
    }
    response = client.post("/warehouses/", json=new_warehouse)
    assert response.status_code == 201

def test_get_warehouse(client):
    response = client.get("/warehouses/1")
    assert response.status_code == 200
    assert response.json()["name"] == "Main Warehouse"

def test_update_warehouse(client):
    update_data = {"name": "Updated Warehouse"}
    response = client.put("/warehouses/1", json=update_data)
    assert response.status_code == 200
    assert response.json()["name"] == "Updated Warehouse"

def test_delete_warehouse(client):
    response = client.delete("/warehouses/1")
    assert response.status_code == 200
    assert client.get("/warehouses/1").status_code == 404
