import pytest
import httpx
import json
from datetime import datetime

MOCK_CLIENTS = {"1": {"id": 1, "name": "Jane Doe", "address": "Second Street 2", "city": "Utrecht"}}

class MockClientDatabase:
    def __init__(self):
        self.clients = MOCK_CLIENTS.copy()

    def create(self, client):
        client_id = str(client["id"])
        if client_id in self.clients:
            return None
        self.clients[client_id] = {**client, "created_at": datetime.now().isoformat()}
        return self.clients[client_id]

    def get(self, client_id):
        return self.clients.get(str(client_id))

    def update(self, client_id, data):
        if client_id in self.clients:
            self.clients[client_id].update(data)
            return self.clients[client_id]
        return None

    def delete(self, client_id):
        return self.clients.pop(str(client_id), None)

@pytest.fixture
def mock_client_db():
    return MockClientDatabase()

@pytest.fixture
def client(mock_client_db):
    client = httpx.Client(base_url="http://localhost:3000")

    def mock_response(request):
        path, method = request.url.path, request.method
        client_id = path.split("/")[-1]

        if "/clients/" in path:
            if method == "POST":
                data = json.loads(request.content)
                client = mock_client_db.create(data)
                return httpx.Response(201 if client else 400, json=client or {"error": "Already exists"})
            elif method == "GET":
                client = mock_client_db.get(client_id)
                return httpx.Response(200 if client else 404, json=client or {"error": "Not found"})
            elif method == "PUT":
                data = json.loads(request.content)
                client = mock_client_db.update(client_id, data)
                return httpx.Response(200 if client else 404, json=client or {"error": "Not found"})
            elif method == "DELETE":
                deleted = mock_client_db.delete(client_id)
                return httpx.Response(200 if deleted else 404, json={"message": "Deleted" if deleted else "Not found"})

        return httpx.Response(404)

    client._transport = httpx.MockTransport(mock_response)
    return client

def test_create_client(client):
    new_client = {"id": 2, "name": "John Smith", "address": "Main Street 3"}
    response = client.post("/clients/", json=new_client)
    assert response.status_code == 201

def test_get_client(client):
    response = client.get("/clients/1")
    assert response.status_code == 200

def test_update_client(client):
    update_data = {"address": "Updated Address"}
    response = client.put("/clients/1", json=update_data)
    assert response.status_code == 200

def test_delete_client(client):
    response = client.delete("/clients/1")
    assert response.status_code == 200
