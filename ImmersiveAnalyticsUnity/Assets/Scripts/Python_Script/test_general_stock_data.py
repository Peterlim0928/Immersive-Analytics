import unittest
from general_stock_data_script import main

class TestGeneralStockData(unittest.TestCase):

    def test_main(self):
        # Assume main() returns the data dictionary
        data = main()

        # Check if the returned data is a dictionary
        self.assertIsInstance(data, dict)

        # Loop through each company symbol (key) in the data
        for company_symbol, company_data in data.items():
            # Check if the value is a dictionary
            self.assertIsInstance(company_data, dict)

            # Check for the presence of expected keys
            self.assertIn("Name", company_data)
            self.assertIn("Previous Price", company_data)
            self.assertIn("Current Price", company_data)
            self.assertIn("Change Figure", company_data)
            self.assertIn("Change Percentage", company_data)
            self.assertIn("Logo URL", company_data)

            # Optionally, check if each key has the expected data type
            self.assertIsInstance(company_data["Name"], str)
            self.assertIsInstance(company_data["Previous Price"], (int, float))
            self.assertIsInstance(company_data["Current Price"], (int, float))
            self.assertIsInstance(company_data["Change Figure"], (int, float))
            self.assertIsInstance(company_data["Change Percentage"], (int, float))
            self.assertIsInstance(company_data["Logo URL"], str)

# To run the test if this file is executed directly
if __name__ == '__main__':
    unittest.main()
