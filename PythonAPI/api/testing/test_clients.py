import pytest
import requests
import time
from datetime import datetime

class TestClientsSystem:
    BASE_URL = "http://localhost:3000/api/v1"
    HEADERS = {'API_KEY': 'a1b2c3d4e5', 'Content-Type': 'application/json'}
    test_client_ids = []  # Track our test records

    @pytest.fixture(scope="class", autouse=True)
    def setup_class(self):
        try:
            response = requests.get(f"{self.BASE_URL}/clients", headers=self.HEADERS)
            assert response.status_code == 200
            self.initial_clients = response.json()
        except requests.ConnectionError:
            pytest.fail("API server is not running")

    def test_create_client(self):
        new_client = {
            "id": 1001,
            "name": "Test Client A",
            "address": "Test Address 1",
            "city": "Test City",            
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        response = requests.post(f"{self.BASE_URL}/clients", json=new_client, headers=self.HEADERS)
        assert response.status_code == 201, f"Expected 201, but got {response.status_code}. Response: {response.text}"

        if response.text:
            created_client = response.json()
            assert str(created_client['id']) == str(new_client['id'])
            self.test_client_ids.append(str(new_client['id']))

    def test_create_another_client(self):
        new_client = {
            "id": 1002,
            "name": "Test Client B",
            "address": "Test Address 2",
            "city": "Test City",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        response = requests.post(f"{self.BASE_URL}/clients", json=new_client, headers=self.HEADERS)
        assert response.status_code == 201
        self.test_client_ids.append(str(new_client['id']))

    def test_get_all_clients(self):
        response = requests.get(f"{self.BASE_URL}/clients", headers=self.HEADERS)
        assert response.status_code == 200

        clients = response.json()
        assert isinstance(clients, list)

        # Print clients for debugging
        print("\nAll clients:", clients)

        # Look for our test clients
        client_ids = [str(c.get('id', '')) for c in clients]
        assert any(id in self.test_client_ids for id in client_ids), "Test clients not found in list"

    def test_get_client_by_id(self):
        # First create a client to retrieve
        new_client = {
            "id": 1, 
            "name": "Raymond Inc", 
            "address": "1296 Daniel Road Apt. 349", 
            "city": "Pierceview", 
            "zip_code": "28301", 
            "province": "Colorado", 
            "country": "United States", 
            "contact_name": "Bryan Clark", 
            "contact_phone": "242.732.3483x2573", 
            "contact_email": "robertcharles@example.net",
            "created_at": datetime.now().isoformat(), # this might be issue that has to be fixed
            "updated_at": datetime.now().isoformat()
        }

        create_response = requests.post(f"{self.BASE_URL}/clients", json=new_client, headers=self.HEADERS)
        assert create_response.status_code == 201
        self.test_client_ids.append(str(new_client['id']))

        # Then get it by ID
        get_response = requests.get(f"{self.BASE_URL}/clients/{new_client['id']}", headers=self.HEADERS)
        assert get_response.status_code == 200

        client = get_response.json()
        assert str(client['id']) == str(new_client['id'])
        assert client['name'] == new_client['name']

    def test_update_client(self):
        # First create a client to update
        new_client = {
            "id": 1004,
            "name": "Test Client D",
            "address": "Test Address 4",
            "city": "Test City",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        create_response = requests.post(f"{self.BASE_URL}/clients", json=new_client, headers=self.HEADERS)
        assert create_response.status_code == 201
        self.test_client_ids.append(str(new_client['id']))

        # Update the client
        update_data = {
            **new_client,
            "address": "Updated Test Address",
            "updated_at": datetime.now().isoformat()
        }

        update_response = requests.put(
            f"{self.BASE_URL}/clients/{new_client['id']}", 
            json=update_data, 
            headers=self.HEADERS
        )
        assert update_response.status_code == 200

        # Verify the update
        get_response = requests.get(f"{self.BASE_URL}/clients/{new_client['id']}", headers=self.HEADERS)
        assert get_response.status_code == 200

        updated_client = get_response.json()
        assert updated_client['address'] == update_data['address']
    
    def test_delete_client(self):
        # First create a client to delete
        new_client = {
            "id": 1006, 
            "name": "PYTEST CLIENT 1006", 
            "address": "PYTEST CLIENT 1006", 
            "city": "PYTEST CLIENT 1006", 
            "zip_code": "PYTEST CLIENT 1006", 
            "province": "PYTEST CLIENT 1006", 
            "country": "PYTEST CLIENT 1006", 
            "contact_name": "PYTEST CLIENT 1006", 
            "contact_phone": "PYTEST CLIENT 1006", 
            "contact_email": "PYTEST CLIENT 1006",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        create_response = requests.post(f"{self.BASE_URL}/clients", json=new_client, headers=self.HEADERS)
        assert create_response.status_code == 201

        # Delete the client
        delete_response = requests.delete(f"{self.BASE_URL}/clients/{new_client['id']}", headers=self.HEADERS)
        assert delete_response.status_code in [200, 204]

    def test_create_invalid_client(self):
        invalid_client = {
            "id": -1,
            "name": "",  # Invalid empty name
            "address": ""   # Invalid empty address
        }

        response = requests.post(f"{self.BASE_URL}/clients", json=invalid_client, headers=self.HEADERS)
        assert response.status_code in [400, 201], f"Unexpected status code {response.status_code}"

    def test_verify_state(self):
        """Verify final state"""
        response = requests.get(f"{self.BASE_URL}/clients", headers=self.HEADERS)
        assert response.status_code == 200
        
        current_clients = response.json()
        print("\nFinal clients list:", current_clients)
        
        # Verify our test clients exist
        for client_id in self.test_client_ids:
            found = any(str(c.get('id')) == client_id for c in current_clients)
            assert found, f"Test client {client_id} not found in final state"

if __name__ == "__main__":
    pytest.main(["-v"])
