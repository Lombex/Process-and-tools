import requests
from datetime import datetime
import pytest

BASE_URL = "http://localhost:3000/api/v1"
HEADERS = {'API_KEY': 'a1b2c3d4e5', 'Content-Type': 'application/json'}

class TestSupplierAPI:

    @pytest.fixture(autouse=True)
    def setup_and_teardown(self):
        # Setup: Create a test supplier to work with
        self.test_supplier = {
            "id": 999,
            "code": "TEST999",
            "name": "Test Supplier",
            "address": "123 Test Lane",
            "address_extra": "Suite 100",
            "city": "Test City",
            "zip_code": "12345",
            "province": "Test Province",
            "country": "Test Country",
            "contact_name": "Test Contact",
            "phonenumber": "555-555-5555",
            "reference": "TS-TEST999",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        # Create supplier for testing
        response = requests.post(f"{BASE_URL}/suppliers", json=self.test_supplier, headers=HEADERS)
        assert response.status_code == 201

        yield

        # Teardown: Delete the test supplier
        requests.delete(f"{BASE_URL}/suppliers/{self.test_supplier['id']}", headers=HEADERS)

    def test_get_all_suppliers(self):
        response = requests.get(f"{BASE_URL}/suppliers", headers=HEADERS)
        assert response.status_code == 200
        assert isinstance(response.json(), list)

    def test_get_supplier_by_id(self):
        get_supplier = {
            "id": 1,
            "code": "SUP0001",
            "name": "Lee, Parks and Johnson",
            "address": "5989 Sullivan Drives",
            "address_extra": "Apt. 996",
            "city": "Port Anitaburgh",
            "zip_code": "91688",
            "province": "Illinois",
            "country": "Czech Republic",
            "contact_name": "Toni Barnett",
            "phonenumber": "363.541.7282x36825",
            "reference": "LPaJ-SUP0001",
            "created_at": "1971-10-20 18:06:17",
            "updated_at": "1985-06-08 00:13:46"
        }

        response = requests.get(f"{BASE_URL}/suppliers/{get_supplier['id']}", headers=HEADERS)
        assert response.status_code == 200
        data = response.json()
        assert data['id'] == get_supplier['id']
        assert data['name'] == get_supplier['name']

    def test_get_supplier_item(self):
        response = requests.get(f"{BASE_URL}/suppliers/{self.test_supplier['id']}/item", headers=HEADERS)
        assert response.status_code in [200, 404]  # Adjust based on expected behavior

    def test_create_supplier(self):
        new_supplier = {
            "id": 1001,
            "code": "NEW001",
            "name": "New Supplier",
            "address": "456 New Lane",
            "address_extra": "Apt. 101",
            "city": "New City",
            "zip_code": "67890",
            "province": "New Province",
            "country": "New Country",
            "contact_name": "New Contact",
            "phonenumber": "666-666-6666",
            "reference": "NS-NEW001",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        response = requests.post(f"{BASE_URL}/suppliers", json=new_supplier, headers=HEADERS)
        assert response.status_code == 201

    import requests
from datetime import datetime
import pytest

BASE_URL = "http://localhost:3000/api/v1"
HEADERS = {'API_KEY': 'a1b2c3d4e5', 'Content-Type': 'application/json'}

class TestSupplierAPI:

    @pytest.fixture(autouse=True)
    def setup_and_teardown(self):
        # Setup: Create a test supplier to work with
        self.test_supplier = {
            "id": 999,
            "code": "TEST999",
            "name": "Test Supplier",
            "address": "123 Test Lane",
            "address_extra": "Suite 100",
            "city": "Test City",
            "zip_code": "12345",
            "province": "Test Province",
            "country": "Test Country",
            "contact_name": "Test Contact",
            "phonenumber": "555-555-5555",
            "reference": "TS-TEST999",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        # Create supplier for testing
        response = requests.post(f"{BASE_URL}/suppliers", json=self.test_supplier, headers=HEADERS)
        assert response.status_code == 201

        yield

        # Teardown: Delete the test supplier
        requests.delete(f"{BASE_URL}/suppliers/{self.test_supplier['id']}", headers=HEADERS)

    def test_get_all_suppliers(self):
        response = requests.get(f"{BASE_URL}/suppliers", headers=HEADERS)
        assert response.status_code == 200
        assert isinstance(response.json(), list)

    def test_get_supplier_by_id(self):
        get_supplier = {
            "id": 1,
            "code": "SUP0001",
            "name": "Lee, Parks and Johnson",
            "address": "5989 Sullivan Drives",
            "address_extra": "Apt. 996",
            "city": "Port Anitaburgh",
            "zip_code": "91688",
            "province": "Illinois",
            "country": "Czech Republic",
            "contact_name": "Toni Barnett",
            "phonenumber": "363.541.7282x36825",
            "reference": "LPaJ-SUP0001",
            "created_at": "1971-10-20 18:06:17",
            "updated_at": "1985-06-08 00:13:46"
        }

        response = requests.get(f"{BASE_URL}/suppliers/{get_supplier['id']}", headers=HEADERS)
        assert response.status_code == 200
        data = response.json()
        assert data['id'] == get_supplier['id']
        assert data['name'] == get_supplier['name']

    def test_get_supplier_item(self):
        response = requests.get(f"{BASE_URL}/suppliers/{self.test_supplier['id']}/item", headers=HEADERS)
        assert response.status_code in [200, 404]  # Adjust based on expected behavior

    def test_create_supplier(self):
        new_supplier = {
            "id": 1001,
            "code": "NEW001",
            "name": "New Supplier",
            "address": "456 New Lane",
            "address_extra": "Apt. 101",
            "city": "New City",
            "zip_code": "67890",
            "province": "New Province",
            "country": "New Country",
            "contact_name": "New Contact",
            "phonenumber": "666-666-6666",
            "reference": "NS-NEW001",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        response = requests.post(f"{BASE_URL}/suppliers", json=new_supplier, headers=HEADERS)
        assert response.status_code == 201

    def test_update_supplier(self):
        updated_supplier = self.test_supplier.copy()
        updated_supplier["name"] = "Updated Supplier Name"

        response = requests.put(f"{BASE_URL}/suppliers/1001", json=updated_supplier, headers=HEADERS)
        assert response.status_code == 200

        # Verify the update
        response = requests.get(f"{BASE_URL}/suppliers/1001", headers=HEADERS)
        data = response.json()
        assert data['name'] == "Updated Supplier Name"

    def test_delete_supplier(self):
        # Delete the supplier
        response = requests.delete(f"{BASE_URL}/suppliers/1001", headers=HEADERS)
        assert response.status_code == 200

        # Verify deletion
        response = requests.get(f"{BASE_URL}/suppliers/1001", headers=HEADERS)
        assert response.status_code == 404



    import requests
from datetime import datetime
import pytest

BASE_URL = "http://localhost:3000/api/v1"
HEADERS = {'API_KEY': 'a1b2c3d4e5', 'Content-Type': 'application/json'}

class TestSupplierAPI:

    @pytest.fixture(autouse=True)
    def setup_and_teardown(self):
        # Setup: Create a test supplier to work with
        self.test_supplier = {
            "id": 999,
            "code": "TEST999",
            "name": "Test Supplier",
            "address": "123 Test Lane",
            "address_extra": "Suite 100",
            "city": "Test City",
            "zip_code": "12345",
            "province": "Test Province",
            "country": "Test Country",
            "contact_name": "Test Contact",
            "phonenumber": "555-555-5555",
            "reference": "TS-TEST999",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        # Create supplier for testing
        response = requests.post(f"{BASE_URL}/suppliers", json=self.test_supplier, headers=HEADERS)
        assert response.status_code == 201

        yield

        # Teardown: Delete the test supplier
        requests.delete(f"{BASE_URL}/suppliers/{self.test_supplier['id']}", headers=HEADERS)

    def test_get_all_suppliers(self):
        response = requests.get(f"{BASE_URL}/suppliers", headers=HEADERS)
        assert response.status_code == 200
        assert isinstance(response.json(), list)

    def test_get_supplier_by_id(self):
        get_supplier = {
            "id": 1,
            "code": "SUP0001",
            "name": "Lee, Parks and Johnson",
            "address": "5989 Sullivan Drives",
            "address_extra": "Apt. 996",
            "city": "Port Anitaburgh",
            "zip_code": "91688",
            "province": "Illinois",
            "country": "Czech Republic",
            "contact_name": "Toni Barnett",
            "phonenumber": "363.541.7282x36825",
            "reference": "LPaJ-SUP0001",
            "created_at": "1971-10-20 18:06:17",
            "updated_at": "1985-06-08 00:13:46"
        }

        response = requests.get(f"{BASE_URL}/suppliers/{get_supplier['id']}", headers=HEADERS)
        assert response.status_code == 200
        data = response.json()
        assert data['id'] == get_supplier['id']
        assert data['name'] == get_supplier['name']

    def test_get_supplier_item(self):
        response = requests.get(f"{BASE_URL}/suppliers/{self.test_supplier['id']}/item", headers=HEADERS)
        assert response.status_code in [200, 404]  # Adjust based on expected behavior

    def test_create_supplier(self):
        new_supplier = {
            "id": 1001,
            "code": "NEW001",
            "name": "New Supplier",
            "address": "456 New Lane",
            "address_extra": "Apt. 101",
            "city": "New City",
            "zip_code": "67890",
            "province": "New Province",
            "country": "New Country",
            "contact_name": "New Contact",
            "phonenumber": "666-666-6666",
            "reference": "NS-NEW001",
            "created_at": datetime.now().isoformat(),
            "updated_at": datetime.now().isoformat()
        }

        response = requests.post(f"{BASE_URL}/suppliers", json=new_supplier, headers=HEADERS)
        assert response.status_code == 201

    # updating supplier

    # deleting supplier
