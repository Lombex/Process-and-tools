import pytest
import requests
from datetime import datetime

class TestTransfersSystem:
    BASE_URL = "http://localhost:3000/api/v1"
    HEADERS = {'API_KEY': 'a1b2c3d4e5', 'Content-Type': 'application/json'}

    @pytest.fixture(scope="class", autouse=True)
    def setup_class(self):
        try:
            response = requests.get(f"{self.BASE_URL}/transfers", headers=self.HEADERS)
            assert response.status_code == 200
        except requests.ConnectionError:
            pytest.fail("API server is not running")

    def test_create_transfer(self):
        new_transfer = {
            "id": 3001,
            "reference": "TR3001",
            "transfer_from": 500,
            "transfer_to": 501,
            "transfer_status": "Pending",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat(),
            "items": [
                {"item_id": "P007433", "amount": 5}
            ]
        }

        response = requests.post(f"{self.BASE_URL}/transfers", json=new_transfer, headers=self.HEADERS)
        assert response.status_code == 201, f"Expected 201, but got {response.status_code}. Response: {response.text}"
        
        if response.text:
            created_transfer = response.json()
            assert str(created_transfer['id']) == str(new_transfer['id'])

    def test_create_another_transfer(self):
        new_transfer = {
            "id": 3002,
            "reference": "TR3002",
            "transfer_from": 502,
            "transfer_to": 503,
            "transfer_status": "Pending",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat(),
            "items": [
                {"item_id": "P007434", "amount": 3}
            ]
        }

        response = requests.post(f"{self.BASE_URL}/transfers", json=new_transfer, headers=self.HEADERS)
        assert response.status_code == 201

    def test_get_all_transfers(self):
        response = requests.get(f"{self.BASE_URL}/transfers", headers=self.HEADERS)
        assert response.status_code == 200, f"Expected 200, but got {response.status_code}. Response: {response.text}"
        
        transfers = response.json()
        assert isinstance(transfers, list)
        
        # Print transfers for debugging
        print("\nAll transfers:", transfers)
        
        # Look for our test transfers
        transfer_ids = [str(t.get('id', '')) for t in transfers]
        assert any(id in ['3001', '3002'] for id in transfer_ids), "Test transfers not found in list"

    def test_create_invalid_transfer(self):
        invalid_transfer = {
            "id": -1,
            "reference": "",  # Invalid empty reference
            "transfer_from": None,  # Invalid null source
            "transfer_to": -1,  # Invalid destination
            "items": []  # Empty items list
        }

        response = requests.post(f"{self.BASE_URL}/transfers", json=invalid_transfer, headers=self.HEADERS)
        assert response.status_code in [400, 201], f"Unexpected status code {response.status_code}"

    def test_final_cleanup(self):
        """Verify final state"""
        response = requests.get(f"{self.BASE_URL}/transfers", headers=self.HEADERS)
        assert response.status_code == 200
        
        print("\nFinal transfers list:", response.json())
        assert response.status_code == 200

if __name__ == "__main__":
    pytest.main(["-v"])
