import { createApp } from 'vue';
import { createPinia } from 'pinia';
import App from './App.vue';
import router from './router';
import Orion from '@orion.ui/orion'; // Import de la librairie Orion
import '@orion.ui/orion/dist/style.css'; // Import des styles
import '@orion.ui/orion/dist/monkey-patching';
import axios from 'axios';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css'; // Import Bootstrap Icons
import 'bootstrap';
import '@fortawesome/fontawesome-free/css/all.css';
import store from './Store'; // Import du store (Vuex?)
import Cookies from 'js-cookie'; // Import de js-cookie pour gérer les cookies
import './assets/theme.css'; // Import des styles de thème
import { useThemeStore } from './Store/theme'; // Import du store de thème

// Créer l'application Vue
const app = createApp(App);

// Initialiser Pinia
const pinia = createPinia();
app.use(pinia);

// Initialiser le thème
const themeStore = useThemeStore();
themeStore.initTheme();

// Définir l'URL de base pour Axios
axios.defaults.baseURL = "http://localhost:5000";

// Add a request interceptor
axios.interceptors.request.use(
  config => {
    const token = Cookies.get('token'); // Récupère le token depuis les cookies
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    if (!(config.data instanceof FormData)) {
      config.headers.Accept = 'application/json';
      config.headers['Content-Type'] = 'application/json';
    }
  
    return config;
  },
  error => {
    console.error('Request Error:', error);
    return Promise.reject(error);
  }
);

// Add a response interceptor
axios.interceptors.response.use(
  response => {
    return response;
  },
  error => {
    console.error('API Error:', {
      url: error.config?.url,
      method: error.config?.method,
      status: error.response?.status,
      data: error.response?.data,
      message: error.message,
      headers: error.config?.headers,
      baseURL: error.config?.baseURL
    });
    return Promise.reject(error);
  }
);

// Utiliser les plugins et le store
app.use(router);
app.use(Orion);
app.use(store);

// Vérifier le statut de l'utilisateur au démarrage de l'application
store.dispatch('checkUserStatus');

// Monter l'application
app.mount('#app');