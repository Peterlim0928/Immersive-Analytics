# Function to take a screenshot using ScreenshotAPI
import requests
import sys

def take_screenshot(url, api_key, output_file):
    url = f"https://api.screenshotone.com/take?access_key={api_key}&url={url}%2F&full_page=true&full_page_scroll=false&viewport_width=1920&viewport_height=1080&device_scale_factor=1&format=jpg&image_quality=80&block_ads=true&block_cookie_banners=true&block_banners_by_heuristics=false&block_trackers=true&delay=0&timeout=60"
    params = None
    method = "GET"
    headers = None
    data = None
    response = requests.request(method, url, headers=headers, params=params, data=data)
    if response.status_code != 200:
        raise Exception(f"Request failed with status {response.status_code}")
    
    # Save the image to a file
    with open(output_file, 'wb') as file:
        file.write(response.content)

if __name__ == "__main__":
    _, url, output_file = sys.argv
    api_key = 'p8eTKK7AzVCziQ'
    # url = 'https://www.fool.com/investing/2024/09/18/3-reasons-this-dow-dividend-growth-stock-could-bla/'
    # output_file = 'webpage_screenshot.png'
    take_screenshot(url, api_key, output_file)
