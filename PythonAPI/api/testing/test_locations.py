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

class TestLocations:
    @pytest.fixture
    def locations(self):
        # Create an instance of Locations with mock data
        loc = Locations(root_path="", is_debug=True)  # Set root_path as needed
        loc.data = mock_locations_data  # Load mock data directly
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
