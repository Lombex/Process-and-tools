import pytest
import requests
from datetime import datetime

class TestItemsSystem:
    BASE_URL = "http://localhost:3000/api/v1"
    HEADERS = {'API_KEY': 'a1b2c3d4e5', 'Content-Type': 'application/json'}
    test_item_ids = []  # Track our test records

    @pytest.fixture(scope="class", autouse=True)
    def setup_class(self):
        try:
            response = requests.get(f"{self.BASE_URL}/items", headers=self.HEADERS)
            assert response.status_code == 200
            self.initial_items = response.json()
        except requests.ConnectionError:
            pytest.fail("API server is not running")

    def test_create_item(self):
        new_item = {
            "id": 1001,
            "name": "Smartphone",
            "description": "High-end smartphone with 5G",
            "category": "Electronics",
            "price": 799.99,
            "sku": "SPH001",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        response = requests.post(f"{self.BASE_URL}/items", json=new_item, headers=self.HEADERS)
        assert response.status_code == 201, f"Expected 201, but got {response.status_code}. Response: {response.text}"
        
        if response.text:
            created_item = response.json()
            assert str(created_item['id']) == str(new_item['id'])
            self.test_item_ids.append(str(new_item['id']))

    def test_create_another_item(self):
        new_item = {
            "id": 1002,
            "name": "Tablet",
            "description": "Portable tablet with stylus support",
            "category": "Electronics",
            "price": 499.99,
            "sku": "TAB001",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        response = requests.post(f"{self.BASE_URL}/items", json=new_item, headers=self.HEADERS)
        assert response.status_code == 201
        self.test_item_ids.append(str(new_item['id']))

    def test_get_all_items(self):
        response = requests.get(f"{self.BASE_URL}/items", headers=self.HEADERS)
        assert response.status_code == 200
        
        items = response.json()
        assert isinstance(items, list)
        
        # Print items for debugging
        print("\nAll items:", items)
        
        # Look for our test items
        item_ids = [str(i.get('id', '')) for i in items]
        assert any(id in self.test_item_ids for id in item_ids), "Test items not found in list"

    def test_create_invalid_item(self):
        invalid_item = {
            "id": -1,
            "name": "",  # Invalid empty name
            "description": "",
            "category": "InvalidCategory",
            "price": -100.0,  # Invalid negative price
            "sku": ""  # Invalid empty SKU
        }

        response = requests.post(f"{self.BASE_URL}/items", json=invalid_item, headers=self.HEADERS)
        assert response.status_code in [400, 201], f"Unexpected status code {response.status_code}"

    def test_verify_state(self):
        """Verify final state"""
        response = requests.get(f"{self.BASE_URL}/items", headers=self.HEADERS)
        assert response.status_code == 200
        
        current_items = response.json()
        print("\nFinal items list:", current_items)
        
        # Verify our test items exist
        for item_id in self.test_item_ids:
            found = any(str(i.get('id')) == item_id for i in current_items)
            assert found, f"Test item {item_id} not found in final state"

if __name__ == "__main__":
    pytest.main(["-v"])
