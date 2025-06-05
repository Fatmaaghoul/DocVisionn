# Use slim-buster for better compatibility
FROM python:3.10-slim-buster

# Set environment variables
ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PIP_NO_CACHE_DIR=1 \
    PIP_DISABLE_PIP_VERSION_CHECK=1 \
    PIP_DEFAULT_TIMEOUT=100 \
    NLTK_DATA=/app/nltk_data \
    YOLO_CONFIG_DIR=/app/ultralytics

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgl1 \
    libglib2.0-0 \
    build-essential \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create necessary directories with correct permissions
RUN mkdir -p /app/nltk_data /app/ultralytics && \
    chown -R appuser:appuser /app

# --- Étape clé pour tirer parti du cache ---
# Copier uniquement requirements.txt d'abord
COPY requirements.txt .

# Installer les dépendances (caché si requirements.txt ne change pas)
RUN pip install --upgrade pip setuptools wheel && \
    pip install -r requirements.txt --retries 10 --timeout 30000 && \
    pip install torch

# Télécharger le modèle YOLOv8x pendant la construction
RUN python -c "from ultralytics import YOLO; YOLO('yolov8x.pt')"

# Télécharger les données NLTK
RUN python -c "import nltk; nltk.data.path = ['/app/nltk_data']; \
    nltk.download('punkt', download_dir='/app/nltk_data'); \
    nltk.download('averaged_perceptron_tagger', download_dir='/app/nltk_data'); \
    nltk.download('maxent_ne_chunker', download_dir='/app/nltk_data'); \
    nltk.download('words', download_dir='/app/nltk_data'); \
    nltk.download('wordnet', download_dir='/app/nltk_data'); \
    nltk.download('omw-1.4', download_dir='/app/nltk_data')" && \
    chown -R appuser:appuser /app/nltk_data

# Copier le reste de l'application
COPY --chown=appuser:appuser . .

# Utiliser l'utilisateur non-root
USER appuser

# Exposer le port
EXPOSE 8000

# Commande de démarrage
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
