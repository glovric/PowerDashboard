from pydantic import BaseModel
from pathlib import Path
import json
from dotenv import load_dotenv
import os
from .logger import logger

PROJECT_ROOT = Path(__file__).resolve().parent.parent
CONFIG_DIR = PROJECT_ROOT.parent / "Shared"
JSON_FILE_PATH = CONFIG_DIR / "sharedsettings.json"
ENV_FILE_PATH = CONFIG_DIR / ".env"

def normalize_keys(obj):
    """Recursively lowercase dictionary keys."""
    if isinstance(obj, dict):
        return {k.lower(): normalize_keys(v) for k, v in obj.items()}
    return obj

def auto_cast(value: str):
    """Convert string env values to int, float, bool when possible."""
    if value.lower() in ("true", "false"):
        return value.lower() == "true"

    if value.isdigit():
        return int(value)

    try:
        return float(value)
    except ValueError:
        return value

def deep_merge(base: dict, override: dict):
    for k, v in override.items():
        if isinstance(v, dict) and k in base:
            deep_merge(base[k], v)
        else:
            base[k] = v
    return base

class Settings(BaseModel):
    powerservice_baseurl: str
    authservice_baseurl: str
    service_api_key: str
    token_expiration_seconds: int
    jwt_key: str
    jwt_issuer: str
    jwt_audience: str

    @staticmethod
    def get_environment():
        return (
            os.getenv("ASPNETCORE_ENVIRONMENT")
            or os.getenv("DOTNET_ENVIRONMENT")
            or "Production"
        )

    @staticmethod
    def load_json(path):
        if not path.exists():
            return {}
        with open(path, "r", encoding="utf-8") as f:
            return normalize_keys(json.load(f))

    @staticmethod
    def load_env_vars():
        config = {}

        for key, value in os.environ.items():
            if "__" not in key:
                continue

            parts = key.lower().split("__")
            current = config

            for part in parts[:-1]:
                current = current.setdefault(part, {})

            current[parts[-1]] = auto_cast(value)

        return config

    @classmethod
    def load_settings_dict(cls):
        config = {}

        # 1) load .env FIRST so env vars become visible
        if ENV_FILE_PATH.exists():
            load_dotenv(ENV_FILE_PATH)
            logger.info(".env file loaded")

        # 2) detect environment
        env = cls.get_environment()
        logger.info(f"Environment: {env}")

        # 3) base config
        base = cls.load_json(JSON_FILE_PATH)
        deep_merge(config, base)

        # 4️⃣ environment override
        env_path = CONFIG_DIR / f"sharedsettings.{env}.json"
        env_config = cls.load_json(env_path)
        deep_merge(config, env_config)

        # 5️⃣ environment variables override everything
        env_vars = cls.load_env_vars()
        deep_merge(config, env_vars)

        return config

    @classmethod
    def load_settings(cls):
        settings = cls.load_settings_dict()

        jwt_section = settings["frontjwtsettings"]
        base_urls = settings["services"]

        return cls(
            powerservice_baseurl=base_urls["powerservice"],
            authservice_baseurl=base_urls["authservice"],
            service_api_key=settings["serviceapikeys"]["inferenceservice"],
            token_expiration_seconds=int(
                settings["servicejwtsettings"]["expirationminutes"]
            ) * 60,
            jwt_key=jwt_section["key"],
            jwt_issuer=jwt_section["issuer"],
            jwt_audience=jwt_section["audience"],
        )

settings = Settings.load_settings()