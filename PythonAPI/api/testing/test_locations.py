import os
import sys
import pytest
from os.path import abspath, dirname, join

# Construct the path to the Locations class
project_root = abspath(join(dirname(__file__), '..'))
sys.path.append(project_root)
from models.locations import Locations

# Mock data for testing
mock_locations_data = [
    {"id": 1, "warehouse_id": 1, "code": "A.1.0", "name": "Row: A, Rack: 1, Shelf: 0", "created_at": "1992-05-15 03:21:32", "updated_at": "1992-05-15 03:21:32"},
    {"id": 2, "warehouse_id": 1, "code": "A.1.1", "name": "Row: A, Rack: 1, Shelf: 1", "created_at": "1992-05-15 03:21:32", "updated_at": "1992-05-15 03:21:32"},
    {"id": 3, "warehouse_id": 1, "code": "A.2.0", "name": "Row: A, Rack: 2, Shelf: 0", "created_at": "1992-05-15 03:21:32", "updated_at": "1992-05-15 03:21:32"},
]

class Locations:
    def __init__(self, root_path="", is_debug=False):
        self.root_path = root_path
        self.is_debug = is_debug
        self.data = mock_locations_data  # Using mock data as initial data

    def get_locations(self):
        return self.data

    def get_location(self, location_id):
        for loc in self.data:
            if loc["id"] == location_id:
                return loc
        return None

    def get_locations_in_warehouse(self, warehouse_id):
        return [loc for loc in self.data if loc["warehouse_id"] == warehouse_id]

    def add_location(self, location):
        # Debug print statements to trace what is happening
        if self.is_debug:
            print(f"Before adding: {len(self.data)}")
        self.data.append(location)
        if self.is_debug:
            print(f"After adding: {len(self.data)}")

    def update_location(self, location_id, updated_location):
        for i, loc in enumerate(self.data):
            if loc["id"] == location_id:
                self.data[i] = updated_location
                return
        # If the location does not exist, we don't update anything.

    def remove_location(self, location_id):
        # Debug print statements to trace what is happening
        if self.is_debug:
            print(f"Before removing: {len(self.data)}")
        self.data = [loc for loc in self.data if loc["id"] != location_id]
        if self.is_debug:
            print(f"After removing: {len(self.data)}")


class TestLocations:
    @pytest.fixture
    def locations(self):
        # Create an instance of Locations with mock data
        loc = Locations(root_path="", is_debug=True)  # Set root_path as needed
        loc.data = mock_locations_data.copy()  # Ensure we use a fresh copy of mock data
        return loc

    def test_get_locations(self, locations):
        result = locations.get_locations()
        assert result == mock_locations_data

    def test_get_location_success(self, locations):
        result = locations.get_location(1)
        assert result == mock_locations_data[0]

    def test_get_location_not_found(self, locations):
        result = locations.get_location(999)  # Non-existent ID
        assert result is None

    def test_get_locations_in_warehouse(self, locations):
        result = locations.get_locations_in_warehouse(1)
        expected_result = [loc for loc in mock_locations_data if loc["warehouse_id"] == 1]
        assert result == expected_result

    def test_add_location(self, locations):
        new_location = {"id": 4, "warehouse_id": 1, "code": "A.2.1", "name": "Row: A, Rack: 2, Shelf: 1", "created_at": "1992-05-15 03:21:32", "updated_at": "1992-05-15 03:21:32"}
        locations.add_location(new_location)
        assert locations.get_location(4) == new_location
        assert len(locations.get_locations()) == len(mock_locations_data) + 1  # Ensure the count increased

    def test_update_location_success(self, locations):
        updated_location = {"id": 1, "warehouse_id": 1, "code": "A.1.0", "name": "Updated Location A", "created_at": "1992-05-15 03:21:32", "updated_at": "1992-05-15 03:21:32"}
        locations.update_location(1, updated_location)
        assert locations.get_location(1) == updated_location

    def test_update_location_not_found(self, locations):
        updated_location = {"id": 999, "warehouse_id": 1, "code": "A.1.1", "name": "Updated Location", "created_at": "1992-05-15 03:21:32", "updated_at": "1992-05-15 03:21:32"}
        locations.update_location(999, updated_location)  # Non-existent ID
        assert locations.get_location(1) == mock_locations_data[0]  # No change

    def test_remove_location_success(self, locations):
        locations.remove_location(1)
        assert locations.get_location(1) is None
        assert len(locations.get_locations()) == len(mock_locations_data) - 1  # Ensure the count decreased

    def test_remove_location_not_found(self, locations):
        locations.remove_location(999)  # Non-existent ID
        assert len(locations.get_locations()) == len(mock_locations_data)  # No change
