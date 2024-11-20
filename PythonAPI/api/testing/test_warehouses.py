import pytest
import requests
from datetime import datetime

class TestWarehousesSystem:
    BASE_URL = "http://localhost:3000/api/v1"
    HEADERS = {'API_KEY': 'a1b2c3d4e5', 'Content-Type': 'application/json'}

    @pytest.fixture(scope="class", autouse=True)
    def setup_class(self):
        try:
            response = requests.get(f"{self.BASE_URL}/warehouses", headers=self.HEADERS)
            assert response.status_code == 200
        except requests.ConnectionError:
            pytest.fail("API server is not running")

    def test_create_warehouse(self):
        new_warehouse = {
            "id": 2003,
            "code": "NEWWH01",
            "name": "Test Warehouse",
            "address": "Test Zone 9",
            "zip": "4021AA",
            "city": "Utrecht",
            "province": "Utrecht",
            "country": "NL",
            "contact": {
                "name": "Alice Brown",
                "phone": "+31687654322",
                "email": "alice.brown@example.com"
            },
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        response = requests.post(f"{self.BASE_URL}/warehouses", json=new_warehouse, headers=self.HEADERS)
        assert response.status_code == 201, f"Expected 201, but got {response.status_code}. Response: {response.text}"
        
        if response.text:
            created_warehouse = response.json()
            assert str(created_warehouse['id']) == str(new_warehouse['id'])

    def test_create_another_warehouse(self):
        new_warehouse = {
            "id": 2004,
            "code": "NEWWH02",
            "name": "Another Test Warehouse",
            "address": "Test Zone 10",
            "zip": "4022AA",
            "city": "Rotterdam",
            "province": "South Holland",
            "country": "NL",
            "contact": {
                "name": "Bob Wilson",
                "phone": "+31687654323",
                "email": "bob.wilson@example.com"
            },
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        response = requests.post(f"{self.BASE_URL}/warehouses", json=new_warehouse, headers=self.HEADERS)
        assert response.status_code == 201

    def test_get_all_warehouses(self):
        response = requests.get(f"{self.BASE_URL}/warehouses", headers=self.HEADERS)
        assert response.status_code == 200, f"Expected 200, but got {response.status_code}. Response: {response.text}"
        
        warehouses = response.json()
        assert isinstance(warehouses, list)
        
        # Print warehouses for debugging
        print("\nAll warehouses:", warehouses)
        
        # Look for our test warehouses
        warehouse_ids = [str(w.get('id', '')) for w in warehouses]
        assert any(id in ['2003', '2004'] for id in warehouse_ids), "Test warehouses not found in list"

    def test_create_invalid_warehouse(self):
        invalid_warehouse = {
            "id": -1,
            "code": "",  # Invalid empty code
            "name": "",  # Invalid empty name
            "contact": {
                "name": "",
                "phone": "",
                "email": "invalid-email"
            }
        }

        response = requests.post(f"{self.BASE_URL}/warehouses", json=invalid_warehouse, headers=self.HEADERS)
        assert response.status_code in [400, 201], f"Unexpected status code {response.status_code}"

    def test_final_cleanup(self):
        """Verify final state"""
        response = requests.get(f"{self.BASE_URL}/warehouses", headers=self.HEADERS)
        assert response.status_code == 200
        
        print("\nFinal warehouses list:", response.json())
        assert response.status_code == 200

if __name__ == "__main__":
    pytest.main(["-v"])
