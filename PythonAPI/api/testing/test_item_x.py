import pytest
import httpx
import json
from datetime import datetime

MOCK_ITEM_TYPES = {"1": {"id": 1, "name": "Electronics", "description": "Category for electronic items"}}
MOCK_ITEM_GROUPS = {"1": {"id": 1, "name": "Premium", "description": "Premium item group"}}
MOCK_ITEM_LINES = {"1": {"id": 1, "name": "Smartphones", "description": "Line for smartphones"}}

class MockItemDatabase:
    def __init__(self, items):
        self.items = items

    def create(self, item):
        item_id = str(item["id"])
        if item_id in self.items:
            return None
        self.items[item_id] = {**item, "created_at": datetime.now().isoformat()}
        return self.items[item_id]

    def get(self, item_id):
        return self.items.get(str(item_id))

    def update(self, item_id, data):
        if item_id in self.items:
            self.items[item_id].update(data)
            self.items[item_id]["updated_at"] = datetime.now().isoformat()
            return self.items[item_id]
        return None

    def delete(self, item_id):
        return self.items.pop(str(item_id), None)

@pytest.fixture
def mock_item_type_db():
    return MockItemDatabase(MOCK_ITEM_TYPES.copy())

@pytest.fixture
def mock_item_group_db():
    return MockItemDatabase(MOCK_ITEM_GROUPS.copy())

@pytest.fixture
def mock_item_line_db():
    return MockItemDatabase(MOCK_ITEM_LINES.copy())

@pytest.fixture
def client(mock_item_type_db, mock_item_group_db, mock_item_line_db):
    client = httpx.Client(base_url="http://localhost:3000")

    def mock_response(request):
        path, method = request.url.path, request.method
        item_id = path.split("/")[-1]

        if "/item_types/" in path:
            if method == "POST":
                data = json.loads(request.content)
                item = mock_item_type_db.create(data)
                return httpx.Response(201 if item else 400, json=item or {"error": "Already exists"})
            elif method == "GET":
                item = mock_item_type_db.get(item_id)
                return httpx.Response(200 if item else 404, json=item or {"error": "Not found"})
            elif method == "PUT":
                data = json.loads(request.content)
                item = mock_item_type_db.update(item_id, data)
                return httpx.Response(200 if item else 404, json=item or {"error": "Not found"})
            elif method == "DELETE":
                deleted = mock_item_type_db.delete(item_id)
                return httpx.Response(200 if deleted else 404, json={"message": "Deleted" if deleted else "Not found"})

        elif "/item_groups/" in path:
            if method == "POST":
                data = json.loads(request.content)
                group = mock_item_group_db.create(data)
                return httpx.Response(201 if group else 400, json=group or {"error": "Already exists"})
            elif method == "GET":
                group = mock_item_group_db.get(item_id)
                return httpx.Response(200 if group else 404, json=group or {"error": "Not found"})
            elif method == "PUT":
                data = json.loads(request.content)
                group = mock_item_group_db.update(item_id, data)
                return httpx.Response(200 if group else 404, json=group or {"error": "Not found"})
            elif method == "DELETE":
                deleted = mock_item_group_db.delete(item_id)
                return httpx.Response(200 if deleted else 404, json={"message": "Deleted" if deleted else "Not found"})

        elif "/item_lines/" in path:
            if method == "POST":
                data = json.loads(request.content)
                line = mock_item_line_db.create(data)
                return httpx.Response(201 if line else 400, json=line or {"error": "Already exists"})
            elif method == "GET":
                line = mock_item_line_db.get(item_id)
                return httpx.Response(200 if line else 404, json=line or {"error": "Not found"})
            elif method == "PUT":
                data = json.loads(request.content)
                line = mock_item_line_db.update(item_id, data)
                return httpx.Response(200 if line else 404, json=line or {"error": "Not found"})
            elif method == "DELETE":
                deleted = mock_item_line_db.delete(item_id)
                return httpx.Response(200 if deleted else 404, json={"message": "Deleted" if deleted else "Not found"})

        return httpx.Response(404)

    client._transport = httpx.MockTransport(mock_response)
    return client

def test_create_item_type(client):
    new_item_type = {"id": 2, "name": "Furniture", "description": "Category for furniture"}
    response = client.post("/item_types/", json=new_item_type)
    assert response.status_code == 201

def test_get_item_type(client):
    response = client.get("/item_types/1")
    assert response.status_code == 200

def test_update_item_type(client):
    update_data = {"description": "Updated description"}
    response = client.put("/item_types/1", json=update_data)
    assert response.status_code == 200

def test_delete_item_type(client):
    response = client.delete("/item_types/1")
    assert response.status_code == 200

def test_create_item_group(client):
    new_item_group = {"id": 2, "name": "Standard", "description": "Standard item group"}
    response = client.post("/item_groups/", json=new_item_group)
    assert response.status_code == 201

def test_get_item_group(client):
    response = client.get("/item_groups/1")
    assert response.status_code == 200

def test_update_item_group(client):
    update_data = {"description": "Updated description"}
    response = client.put("/item_groups/1", json=update_data)
    assert response.status_code == 200

def test_delete_item_group(client):
    response = client.delete("/item_groups/1")
    assert response.status_code == 200

def test_create_item_line(client):
    new_item_line = {"id": 2, "name": "Tablets", "description": "Line for tablets"}
    response = client.post("/item_lines/", json=new_item_line)
    assert response.status_code == 201

def test_get_item_line(client):
    response = client.get("/item_lines/1")
    assert response.status_code == 200

def test_update_item_line(client):
    update_data = {"description": "Updated description"}
    response = client.put("/item_lines/1", json=update_data)
    assert response.status_code == 200

def test_delete_item_line(client):
    response = client.delete("/item_lines/1")
    assert response.status_code == 200
