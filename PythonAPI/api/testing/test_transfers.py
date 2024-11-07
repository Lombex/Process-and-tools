import pytest
import httpx
import json
from datetime import datetime

MOCK_TRANSFERS = {
    "1": {
        "id": 1,
        "reference": "TR00001",
        "transfer_from": 9113,
        "transfer_to": 9229,
        "transfer_status": "Completed",
        "created_at": "2000-03-11T13:11:14Z",
        "updated_at": "2000-03-12T16:11:14Z",
        "items": [
            {
                "item_id": "P007435",
                "amount": 23
            }
        ]
    }
}

class MockTransferDatabase:
    def __init__(self):
        self.transfers = MOCK_TRANSFERS.copy()

    def create(self, transfer):
        transfer_id = str(transfer["id"])
        if transfer_id in self.transfers:
            return None
        self.transfers[transfer_id] = {**transfer, "created_at": datetime.now().isoformat()}
        return self.transfers[transfer_id]

    def get(self, transfer_id):
        return self.transfers.get(str(transfer_id))

    def update(self, transfer_id, data):
        transfer_id_str = str(transfer_id)
        if transfer_id_str in self.transfers:
            self.transfers[transfer_id_str].update(data)
            self.transfers[transfer_id_str]["updated_at"] = datetime.now().isoformat()
            return self.transfers[transfer_id_str]
        return None

    def delete(self, transfer_id):
        return self.transfers.pop(str(transfer_id), None)

@pytest.fixture
def mock_transfer_db():
    return MockTransferDatabase()

@pytest.fixture
def client(mock_transfer_db):
    client = httpx.Client(base_url="http://localhost:3000")

    def mock_response(request):
        path, method = request.url.path, request.method
        transfer_id = path.split("/")[-1]

        if "/transfers/" in path:
            if method == "POST":
                data = json.loads(request.content)
                transfer = mock_transfer_db.create(data)
                return httpx.Response(201 if transfer else 400, json=transfer or {"error": "Already exists"})
            elif method == "GET":
                transfer = mock_transfer_db.get(transfer_id)
                return httpx.Response(200 if transfer else 404, json=transfer or {"error": "Not found"})
            elif method == "PUT":
                data = json.loads(request.content)
                transfer = mock_transfer_db.update(transfer_id, data)
                return httpx.Response(200 if transfer else 404, json=transfer or {"error": "Not found"})
            elif method == "DELETE":
                deleted = mock_transfer_db.delete(transfer_id)
                return httpx.Response(200 if deleted else 404, json={"message": "Deleted" if deleted else "Not found"})

        return httpx.Response(404)

    client._transport = httpx.MockTransport(mock_response)
    return client

def test_create_transfer(client):
    new_transfer = {
        "id": 2,
        "reference": "TR00002",
        "transfer_from": 9113,
        "transfer_to": 9229,
        "transfer_status": "Pending",
        "created_at": "2022-01-01T10:00:00Z",
        "updated_at": "2022-01-02T10:00:00Z",
        "items": [
            {"item_id": "P009557", "amount": 10}
        ]
    }
    response = client.post("/transfers/", json=new_transfer)
    assert response.status_code == 201

def test_get_transfer(client):
    response = client.get("/transfers/1")
    assert response.status_code == 200
    assert response.json()["reference"] == "TR00001"

def test_get_incorrect_transfer(client):
    response = client.get("/transfers/-77")
    assert response.status_code == 404

def test_update_transfer(client):
    updated_data = {"transfer_status": "Pending"}
    response = client.put("/transfers/1", json=updated_data)
    assert response.status_code == 200
    assert response.json()["transfer_status"] == "Pending"

def test_update_incorrect_transfer(client):
    updated_data = {"transfer_status": "Pending"}
    response = client.put("/transfers/-15", json=updated_data)
    assert response.status_code == 404

def test_delete_transfer(client):
    response = client.delete("/transfers/1")
    assert response.status_code == 200
    assert client.get("/transfers/1").status_code == 404

def test_delete_incorrect_transfer(client):
    response = client.delete("/transfers/-44")
    assert response.status_code == 404

def test_partial_update_transfer(client):
    updated_data = {"transfer_status": "In Progress"}
    response = client.put("/transfers/1", json=updated_data)
    assert response.status_code == 200
    assert response.json()["transfer_status"] == "In Progress"

def test_create_multiple_transfers(client):
    transfer_2 = {
        "id": 2,
        "reference": "TR00002",
        "transfer_from": 9113,
        "transfer_to": 9229,
        "transfer_status": "Pending",
        "created_at": "2022-01-01T10:00:00Z",
        "updated_at": "2022-01-02T10:00:00Z",
        "items": [
            {"item_id": "P009557", "amount": 10}
        ]
    }
    response_2 = client.post("/transfers/", json=transfer_2)
    assert response_2.status_code == 201
    assert response_2.json()["reference"] == "TR00002"

def test_get_all_transfers(client):
    transfer_2 = {
        "id": 2,
        "reference": "TR00002",
        "transfer_from": 9113,
        "transfer_to": 9229,
        "transfer_status": "Pending",
        "created_at": "2022-01-01T10:00:00Z",
        "updated_at": "2022-01-02T10:00:00Z",
        "items": [
            {"item_id": "P009557", "amount": 10}
        ]
    }
    client.post("/transfers/", json=transfer_2)
    response = client.get("/transfers/1")
    response_2 = client.get("/transfers/2")
    assert response.status_code == 200
    assert response_2.status_code == 200

def test_delete_multiple_transfers(client):
    transfer_2 = {
        "id": 2,
        "reference": "TR00002",
        "transfer_from": 9113,
        "transfer_to": 9229,
        "transfer_status": "Pending",
        "created_at": "2022-01-01T10:00:00Z",
        "updated_at": "2022-01-02T10:00:00Z",
        "items": [
            {"item_id": "P009557", "amount": 10}
        ]
    }
    client.post("/transfers/", json=transfer_2)
    response = client.delete("/transfers/1")
    response_2 = client.delete("/transfers/2")
    assert response.status_code == 200
    assert response_2.status_code == 200
    assert client.get("/transfers/1").status_code == 404
    assert client.get("/transfers/2").status_code == 404

def test_create_transfer_with_no_items(client):
    response = client.post("/transfers/", json={
        "id": 3,
        "reference": "TR00003",
        "transfer_from": 9113,
        "transfer_to": 9229,
        "transfer_status": "Pending",
        "created_at": "2022-01-01T10:00:00Z",
        "updated_at": "2022-01-02T10:00:00Z"
    })
    assert response.status_code == 201
    assert response.json()["id"] == 3
    assert response.json().get("items") is None

def test_update_transfer_with_multiple_items(client):
    updated_items = [
        {"item_id": "P007435", "amount": 30},
        {"item_id": "P009557", "amount": 5}
    ]
    response = client.put("/transfers/1", json={"items": updated_items})
    assert response.status_code == 200
    assert len(response.json()["items"]) == 2
    assert response.json()["items"][0]["amount"] == 30

def test_update_transfer_status_only(client):
    updated_data = {"transfer_status": "Delivered"}
    response = client.put("/transfers/1", json=updated_data)
    assert response.status_code == 200
    assert response.json()["transfer_status"] == "Delivered"

def test_create_transfer_with_duplicate_item_ids(client):
    response = client.post("/transfers/", json={
        "id": 4,
        "reference": "TR00004",
        "transfer_from": 9113,
        "transfer_to": 9229,
        "transfer_status": "Pending",
        "created_at": "2022-02-01T10:00:00Z",
        "updated_at": "2022-02-02T10:00:00Z",
        "items": [
            {"item_id": "P007435", "amount": 10},
            {"item_id": "P007435", "amount": 5}
        ]
    })
    assert response.status_code == 201
    assert len(response.json()["items"]) == 2
    assert response.json()["items"][0]["item_id"] == "P007435"

def test_delete_transfer_with_items(client):
    response = client.delete("/transfers/1")
    assert response.status_code == 200
    assert client.get("/transfers/1").status_code == 404
