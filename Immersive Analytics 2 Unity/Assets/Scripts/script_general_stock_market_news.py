import requests
import json
from datetime import datetime, timezone, timedelta


def convert_to_readable_format(date_str):
    input_time = datetime.strptime(date_str, '%Y%m%dT%H%M%S')
    
    current_time = datetime.now()
    
    time_diff = current_time - input_time
    
    days_diff = time_diff.days
    hours_diff = time_diff.seconds // 3600
    
    formatted_date = input_time.strftime('%d %B %Y')
    
    if days_diff >= 1:
        return f"{formatted_date} - {days_diff} days ago"
    else:
        return f"{formatted_date} - {hours_diff} hours ago"

def process_news_data(raw_data):
    all_news = raw_data["feed"]
    processed_data = []

    for news in all_news:
        news_data = {
            "title": news["title"],
            "source": news["source"],
            "sentiment_label": news["overall_sentiment_label"],
            "time_published": convert_to_readable_format(news["time_published"]),
            "image_url": news["banner_image"] if news["banner_image"] != None else "None",
        }
        processed_data.append(news_data)
    return processed_data


def main():
    ALPHAVANTAGE_API_KEY = "M2123YJCK0782MJE"
    now = datetime.now(timezone.utc)
    one_day_earlier = now - timedelta(days=1)
    formattedDate = one_day_earlier.strftime("%Y%m%dT%H%M")
    url = f"https://www.alphavantage.co/query?function=NEWS_SENTIMENT&topics=financial_markets&time_from={formattedDate}&limit=3&apikey={ALPHAVANTAGE_API_KEY}"

    try:
        r = requests.get(url)
        data = r.json()
        processed_data = process_news_data(data)
        print(processed_data)
    except Exception as e:
        print(e)

if __name__ == "__main__":
    main()