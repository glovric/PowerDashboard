import pandas as pd
from pathlib import Path

script_dir = Path(__file__).resolve().parent

path15 = script_dir / "time_series_15min_singleindex.csv"
path60 = script_dir / "time_series_60min_singleindex.csv"

target15 = script_dir / "time_series_15.csv"
target60 = script_dir / "time_series_60.csv"

df15 = pd.read_csv(path15)
df60 = pd.read_csv(path60)

countries_15 = list(set(c[:2].upper() for c in df15.columns if "timestamp" not in c))
countries_15.sort()

countries_60 = set(c[:2].upper() for c in df60.columns if "timestamp" not in c)
countries_60.remove("GB")
countries_60.add("GB_GBN")
countries_60 = list(countries_60)
countries_60.sort()

filtered_columns_15 = ["utc_timestamp"] + [f'{c}_load_actual_entsoe_transparency' for c in countries_15]
filtered_columns_60 = ["utc_timestamp"] + [f'{c}_load_actual_entsoe_transparency' for c in countries_60]

df15 = df15[filtered_columns_15]
df60 = df60[filtered_columns_60]

df60.rename(columns={"GB_GBN_load_actual_entsoe_transparency": "GB_load_actual_entsoe_transparency"}, inplace=True)

print(f'Filtering time series CSV files.\n')

df15.to_csv(target15, index=False)
df60.to_csv(target60, index=False)