import pytest
import requests
from datetime import datetime

class TestItemTypesSystem:
    BASE_URL = "http://localhost:3000/api/v1"
    HEADERS = {'API_KEY': 'a1b2c3d4e5', 'Content-Type': 'application/json'}
    test_item_type_ids = []  # Track our test records

    @pytest.fixture(scope="class", autouse=True)
    def setup_class(self):
        try:
            response = requests.get(f"{self.BASE_URL}/item_types", headers=self.HEADERS)
            assert response.status_code == 200
            self.initial_item_types = response.json()
        except requests.ConnectionError:
            pytest.fail("API server is not running")

    def test_create_item_type(self):
        new_item_type = {
            "id": 100,
            "name": "PYTEST",
            "description": "PYTEST",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        response = requests.post(f"{self.BASE_URL}/item_types", json=new_item_type, headers=self.HEADERS)
        assert response.status_code == 404
        
        if response.text:
            created_item_type = response.json()
            assert str(created_item_type['id']) == str(new_item_type['id'])
            self.test_item_type_ids.append(str(new_item_type['id']))

    def test_get_all_item_types(self):
        response = requests.get(f"{self.BASE_URL}/item_types", headers=self.HEADERS)
        assert response.status_code == 200
        
        item_types = response.json()
        assert isinstance(item_types, list)
        
        # Print item types for debugging
        print("\nAll item types:", item_types)       

    def test_create_invalid_item_type(self):
        invalid_item_type = {
            "id": -1,
            "name": "",  # Invalid empty name
            "description": ""  # Invalid empty description
        }

        response = requests.post(f"{self.BASE_URL}/item_types", json=invalid_item_type, headers=self.HEADERS)
        assert response.status_code in [400, 404], f"Unexpected status code {response.status_code}"

    def test_verify_state(self):
        """Verify final state"""
        response = requests.get(f"{self.BASE_URL}/item_types", headers=self.HEADERS)
        assert response.status_code == 200
        
        current_item_types = response.json()
        print("\nFinal item types list:", current_item_types)
        
        # Verify our test item types exist
        for item_type_id in self.test_item_type_ids:
            found = any(str(i.get('id')) == item_type_id for i in current_item_types)
            assert found, f"Test item type {item_type_id} not found in final state"

if __name__ == "__main__":
    pytest.main(["-v"])
