# Use an official Python runtime as the base image
FROM python:3.10-slim

# Set working directory
WORKDIR /app

# Install uv
RUN pip install --no-cache-dir uv

# Copy dependency files first (for better caching)
COPY inferenceservice/pyproject.toml ./pyproject.toml
COPY inferenceservice/uv.lock ./uv.lock
COPY inferenceservice/.python-version ./.python-version

# Install dependencies using uv
RUN uv sync --frozen

# Copy the rest of the application code
COPY inferenceservice/. .
COPY Shared/sharedsettings.json /Shared/sharedsettings.json

# Expose the port that FastAPI will run on
EXPOSE 8080

# Run the FastAPI application using uvicorn
CMD ["uv", "run", "uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8080"]