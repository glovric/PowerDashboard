from enum import Enum, IntEnum

class Interval(IntEnum):
    HOUR = 60
    QUARTER = 15

class ModelType(Enum):
    NOWCAST = "Nowcast"
    FORECAST = "Forecast"

class Country(Enum):
    AT = "AT" 
    BE = "BE"
    BG = "BG"
    CH = "CH"
    CY = "CY"
    CZ = "CZ"
    DE = "DE"
    DK = "DK"
    EE = "EE"
    ES = "ES"
    FI = "FI"
    FR = "FR"
    GB = "GB"
    GR = "GR"
    HR = "HR"
    HU = "HU"
    IE = "IE"
    IT = "IT"
    LT = "LT"
    LU = "LU"
    LV = "LV"
    ME = "ME"
    NL = "NL"
    NO = "NO"
    PL = "PL"
    PT = "PT"
    RO = "RO"
    RS = "RS"
    SE = "SE"
    SI = "SI"
    SK = "SK"
    UA = "UA"

class ForecastHorizon(IntEnum):
    H6 = 6
    H12 = 12
    H24 = 24

def parse_interval(value: int) -> Interval:
    try:
        return Interval(value)
    except ValueError:
        raise ValueError(f"Invalid interval: {value}. Expected 15 or 60.")
    
def parse_country(value: str) -> Country:
    try:
        return Country(value.upper())
    except ValueError:
        raise ValueError(f"Invalid country: {value}.")
    
def parse_forecast_horizon(value: int) -> ForecastHorizon:
    try:
        return ForecastHorizon(value)
    except ValueError:
        raise ValueError(f"Invalid forecast horizon: {value}.")