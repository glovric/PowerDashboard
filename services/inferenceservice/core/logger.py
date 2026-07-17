import logging
from logging.handlers import TimedRotatingFileHandler
from pathlib import Path
import os

def namer(name: str) -> str:
    base_dir = os.path.dirname(name)
    filename = os.path.basename(name)
    date_part = filename.split(".")[-1]
    return os.path.join(base_dir, f"app-{date_part}.log")

def setup_logger(log_dir: str = "logs") -> logging.Logger:

    log_path = Path(log_dir)
    log_path.mkdir(parents=True, exist_ok=True)
    
    log_file = log_path / "app.log"

    logger = logging.getLogger("global_logger")
    logger.setLevel(logging.INFO)

    file_handler = TimedRotatingFileHandler(
        filename=log_file,
        when='midnight',
        interval=1,
        backupCount=7,
        encoding='utf-8',
        delay=True
    )

    file_handler.namer = namer

    formatter = logging.Formatter(
        fmt="%(asctime)s - %(module)s.%(funcName)s - %(levelname)s - %(message)s",
        datefmt="%Y-%m-%d %H:%M:%S"
    )

    file_handler.setFormatter(formatter)
    logger.addHandler(file_handler)

    logger.propagate = False

    return logger

logger = setup_logger()