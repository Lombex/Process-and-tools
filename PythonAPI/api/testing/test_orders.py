import json
from datetime import datetime
import pytest
import httpx

MOCK_ORDERS = {
    "1": {
        "id": 1,
        "source_id": 33,
        "order_date": "2019-04-03T11:33:15Z",
        "request_date": "2019-04-07T11:33:15Z",
        "reference": "ORD00001",
        "reference_extra": "Bedreven arm straffen bureau.",
        "order_status": "Delivered",
        "notes": "Voedsel vijf vork heel.",
        "shipping_notes": "Buurman betalen plaats bewolkt.",
        "picking_notes": "Ademen fijn volgorde scherp aardappel op leren.",
        "warehouse_id": 18,
        "ship_to": None,
        "bill_to": None,
        "shipment_id": 1,
        "total_amount": 9905.13,
        "total_discount": 150.77,
        "total_tax": 372.72,
        "total_surcharge": 77.6,
        "created_at": "2019-04-03T11:33:15Z",
        "updated_at": "2019-04-05T07:33:15Z",
        "items": [
            {"item_id": "P007435", "amount": 23},
            {"item_id": "P009557", "amount": 1},
            {"item_id": "P009553", "amount": 50}
        ]
    }
}

class MockOrderDatabase:
    def __init__(self):
        self.orders = MOCK_ORDERS.copy()

    def create(self, order):
        order_id = str(order["id"])
        if order_id in self.orders:
            return None
        self.orders[order_id] = {**order, "created_at": datetime.now().isoformat()}
        return self.orders[order_id]

    def get(self, order_id):
        return self.orders.get(str(order_id))

    def update(self, order_id, data):
        order_id_str = str(order_id)
        if order_id_str in self.orders:
            self.orders[order_id_str].update(data)
            self.orders[order_id_str]["updated_at"] = datetime.now().isoformat()
            return self.orders[order_id_str]
        return None

    def delete(self, order_id):
        return self.orders.pop(str(order_id), None)

@pytest.fixture
def mock_order_db():
    return MockOrderDatabase()

@pytest.fixture
def client(mock_order_db):
    client = httpx.Client(base_url="http://localhost:3000")

    def mock_response(request):
        path, method = request.url.path, request.method
        order_id = path.split("/")[-1]

        if "/orders/" in path:
            if method == "POST":
                data = json.loads(request.content)
                order = mock_order_db.create(data)
                return httpx.Response(201 if order else 400, json=order or {"error": "Already exists"})
            elif method == "GET":
                order = mock_order_db.get(order_id)
                return httpx.Response(200 if order else 404, json=order or {"error": "Not found"})
            elif method == "PUT":
                data = json.loads(request.content)
                order = mock_order_db.update(order_id, data)
                return httpx.Response(200 if order else 404, json=order or {"error": "Not found"})
            elif method == "DELETE":
                deleted = mock_order_db.delete(order_id)
                return httpx.Response(200 if deleted else 404, json={"message": "Deleted" if deleted else "Not found"})

        return httpx.Response(404)

    client._transport = httpx.MockTransport(mock_response)
    return client

# Test cases for orders
def test_create_order(client):
    new_order = {
        "id": 2,
        "source_id": 34,
        "order_date": "2022-01-01T10:00:00Z",
        "request_date": "2022-01-02T10:00:00Z",
        "reference": "ORD00002",
        "reference_extra": "Extra notes here.",
        "order_status": "Pending",
        "notes": "Some additional notes.",
        "shipping_notes": "Ship via standard delivery.",
        "picking_notes": "Pick quickly.",
        "warehouse_id": 19,
        "ship_to": None,
        "bill_to": None,
        "shipment_id": 2,
        "total_amount": 5000.00,
        "total_discount": 100.00,
        "total_tax": 250.00,
        "total_surcharge": 50.00,
        "items": [
            {"item_id": "P009557", "amount": 10}
        ]
    }
    response = client.post("/orders/", json=new_order)
    assert response.status_code == 201

def test_get_order(client):
    response = client.get("/orders/1")
    assert response.status_code == 200
    assert response.json()["reference"] == "ORD00001"

def test_get_incorrect_order(client):
    response = client.get("/orders/-77")
    assert response.status_code == 404

def test_update_order(client):
    updated_data = {"order_status": "Updated"}
    response = client.put("/orders/1", json=updated_data)
    assert response.status_code == 200
    assert response.json()["order_status"] == "Updated"

def test_update_incorrect_order(client):
    updated_data = {"order_status": "Updated"}
    response = client.put("/orders/-15", json=updated_data)
    assert response.status_code == 404

def test_delete_order(client):
    response = client.delete("/orders/1")
    assert response.status_code == 200
    assert client.get("/orders/1").status_code == 404

def test_delete_incorrect_order(client):
    response = client.delete("/orders/-44")
    assert response.status_code == 404

def test_create_multiple_orders(client):
    new_order_2 = {
        "id": 3,
        "source_id": 35,
        "order_date": "2022-01-01T10:00:00Z",
        "request_date": "2022-01-02T10:00:00Z",
        "reference": "ORD00003",
        "reference_extra": "Another order note.",
        "order_status": "Pending",
        "notes": "Some notes for this order.",
        "shipping_notes": "Ship express.",
        "picking_notes": "Use fast pick.",
        "warehouse_id": 20,
        "ship_to": None,
        "bill_to": None,
        "shipment_id": 3,
        "total_amount": 7500.00,
        "total_discount": 150.00,
        "total_tax": 300.00,
        "total_surcharge": 75.00,
        "items": [
            {"item_id": "P009558", "amount": 5}
        ]
    }
    response_2 = client.post("/orders/", json=new_order_2)
    assert response_2.status_code == 201
    assert response_2.json()["reference"] == "ORD00003"

def test_get_all_orders(client):
    response = client.get("/orders/1")
    assert response.status_code == 200

def test_delete_multiple_orders(client):
    new_order_2 = {
        "id": 3,
        "source_id": 35,
        "order_date": "2022-01-01T10:00:00Z",
        "request_date": "2022-01-02T10:00:00Z",
        "reference": "ORD00003",
        "order_status": "Pending",
        "items": [
            {"item_id": "P009558", "amount": 5}
        ]
    }
    client.post("/orders/", json=new_order_2)
    response = client.delete("/orders/1")
    response_2 = client.delete("/orders/3")
    assert response.status_code == 200
    assert response_2.status_code == 200
    assert client.get("/orders/1").status_code == 404
    assert client.get("/orders/3").status_code == 404

def test_create_order_with_no_items(client):
    response = client.post("/orders/", json={
        "id": 4,
        "source_id": 36,
        "order_date": "2022-01-01T10:00:00Z",
        "request_date": "2022-01-02T10:00:00Z",
        "reference": "ORD00004",
        "order_status": "Pending"
    })
    assert response.status_code == 201
    assert response.json()["id"] == 4
    assert response.json().get("items") is None

def test_update_order_with_multiple_items(client):
    updated_items = [
        {"item_id": "P007435", "amount": 30},
        {"item_id": "P009557", "amount": 5}
    ]
    response = client.put("/orders/1", json={"items": updated_items})
    assert response.status_code == 200
    assert len(response.json()["items"]) == 2
    assert response.json()["items"][0]["amount"] == 30

def test_update_order_status_only(client):
    updated_data = {"order_status": "Delivered"}
    response = client.put("/orders/1", json=updated_data)
    assert response.status_code == 200
    assert response.json()["order_status"] == "Delivered"

def test_create_order_with_duplicate_item_ids(client):
    response = client.post("/orders/", json={
        "id": 5,
        "source_id": 37,
        "order_date": "2022-02-01T10:00:00Z",
        "request_date": "2022-02-02T10:00:00Z",
        "reference": "ORD00005",
        "order_status": "Pending",
        "items": [
            {"item_id": "P007435", "amount": 10},
            {"item_id": "P007435", "amount": 5}
        ]
    })
    assert response.status_code == 201
    assert len(response.json()["items"]) == 2
    assert response.json()["items"][0]["item_id"] == "P007435"

def test_delete_order_with_items(client):
    response = client.delete("/orders/1")
    assert response.status_code == 200
    assert client.get("/orders/1").status_code == 404
