import requests
from bs4 import BeautifulSoup
import sys

# Function to scrape content from an article URL
def scrape_article(url):
    # Send a GET request to the URL
    response = requests.get(url)
    
    # Check if the request was successful
    if response.status_code != 200:
        print(f"Failed to retrieve the article: {response.status_code}")
        return None
    
    # Parse the content of the page with BeautifulSoup
    soup = BeautifulSoup(response.content, 'html.parser')
    
    # Find the main content of the article (specific tags or class names vary by website)
    article_body = soup.find_all(['p', 'h1', 'h2', 'h3'])
    
    # Join the text from the selected tags
    article_text = "\n".join([element.get_text() for element in article_body])
    
    return article_text

if __name__ == "__main__":
    # _, url= sys.argv
    url = "https://www.fool.com/investing/2024/09/18/3-reasons-this-dow-dividend-growth-stock-could-bla/"
    text = scrape_article(url)
    #export as txt
    with open("article.txt", "w") as file:
        file.write(text)