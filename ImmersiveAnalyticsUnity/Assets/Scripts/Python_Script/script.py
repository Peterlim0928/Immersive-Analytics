import sys
import yfinance as yf
import pandas as pd

def main(stock_code: str, time_period: str, time_interval: str = "30m"):
    stock_code = stock_code.upper()
    stock_data = yf.Ticker(stock_code)
    hist = stock_data.history(period = time_period, interval = time_interval)
    pd.DataFrame(hist).to_csv(f"./Assets/Datasets/{stock_code}-{time_period}.csv")


if __name__ == "__main__":
    _, stock_code, time_period, time_inveral = sys.argv
    main(stock_code, time_period, time_inveral)