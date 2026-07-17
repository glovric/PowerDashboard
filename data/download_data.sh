#!/usr/bin/env bash

DATA_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$DATA_DIR/.." && pwd)"
PYTHON="$PROJECT_ROOT/.venv/bin/python"
SCRIPT="$DATA_DIR/filtercsv.py"

if [ ! -d "$DATA_DIR" ]; then
    mkdir -p "$DATA_DIR"
fi

URLS=(
    "https://data.open-power-system-data.org/time_series/2020-10-06/time_series_15min_singleindex.csv"
    "https://data.open-power-system-data.org/time_series/2020-10-06/time_series_60min_singleindex.csv"
)

for URL in "${URLS[@]}"; do
    FILE_NAME=$(basename "$URL")

    OUTPUT_PATH="${DATA_DIR}/${FILE_NAME}"

    echo "Downloading $URL -> $OUTPUT_PATH ..."
    curl -L -o "$OUTPUT_PATH" "$URL"
    echo "✅ Done."
    echo
done

"$PYTHON" "$SCRIPT"