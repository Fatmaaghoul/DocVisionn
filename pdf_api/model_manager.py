import os
import requests
import json
from dotenv import load_dotenv
from typing import Optional, List, Dict
import threading

class ModelManager:
    def __init__(self, base_url: str = None):
        load_dotenv()
        ollama_host = os.getenv("OLLAMA_HOST", "localhost")
        ollama_port = os.getenv("OLLAMA_PORT", "11434")
        self.base_url = base_url or f"http://{ollama_host}:{ollama_port}"
        self._current_model = None
        self._download_thread = None
        self._cancel_download = False
        self._download_status = {"status": "idle", "progress": 0, "message": ""}

    def _download_model_thread(self, model_name: str):
        try:
            self._download_status = {
                "status": "downloading",
                "progress": 0,
                "message": "Début du téléchargement"
            }

            response = requests.post(
                f"{self.base_url}/api/pull",
                json={"name": model_name},
                stream=True
            )
            response.raise_for_status()

            for line in response.iter_lines():
                if self._cancel_download:
                    self._download_status = {
                        "status": "cancelled",
                        "progress": 0,
                        "message": "Téléchargement annulé"
                    }
                    return

                if line:
                    try:
                        data = json.loads(line.decode('utf-8'))
                        if "status" in data:
                            self._download_status["message"] = data["status"]
                        if "total" in data and "completed" in data:
                            progress = int((data["completed"] / data["total"]) * 100)
                            self._download_status["progress"] = progress
                    except Exception:
                        pass

            self._download_status = {
                "status": "completed",
                "progress": 100,
                "message": "Téléchargement terminé"
            }

        except requests.exceptions.RequestException as e:
            self._download_status = {
                "status": "error",
                "progress": 0,
                "message": str(e)
            }

    def download_model(self, model_name: str) -> Dict:
        """
        Lance le téléchargement du modèle dans un thread
        """
        if self._download_thread and self._download_thread.is_alive():
            return {
                "status": "error",
                "message": "Un téléchargement est déjà en cours"
            }

        self._cancel_download = False
        self._current_model = model_name
        self._download_thread = threading.Thread(
            target=self._download_model_thread,
            args=(model_name,),
            daemon=True
        )
        self._download_thread.start()

        return {
            "status": "started",
            "message": f"Téléchargement du modèle '{model_name}' lancé"
        }

    def cancel_download(self) -> Dict:
        if not self._download_thread or not self._download_thread.is_alive():
            return {
                "status": "error",
                "message": "Aucun téléchargement en cours"
            }

        self._cancel_download = True
        return {
            "status": "cancelling",
            "message": "Annulation du téléchargement en cours"
        }

    def get_download_status(self) -> Dict:
        status_with_name = self._download_status.copy()
        status_with_name["model_name"] = self._current_model
        return status_with_name


    def is_model_available(self, model_name: str) -> bool:
        try:
            response = requests.get(f"{self.base_url}/api/tags")
            response.raise_for_status()
            models = response.json().get("models", [])
            return any(model["name"] == model_name for model in models)
        except requests.exceptions.RequestException:
            return False

    def set_active_model(self, model_name: str) -> Dict:
        if not self.is_model_available(model_name):
            return {
                "status": "error",
                "message": f"Le modèle {model_name} n'est pas disponible. Veuillez le télécharger d'abord."
            }

        self._current_model = model_name
        return {
            "status": "success",
            "message": f"Le modèle {model_name} a été défini comme modèle actif."
        }

    def get_active_model(self) -> Optional[str]:
        return self._current_model

    def list_available_models(self) -> List[Dict]:
        try:
            response = requests.get(f"{self.base_url}/api/tags")
            response.raise_for_status()
            models = response.json().get("models", [])
            return [
                {
                    "name": model["name"],
                    "is_active": model["name"] == self._current_model
                }
                for model in models
            ]
        except requests.exceptions.RequestException:
            return []
