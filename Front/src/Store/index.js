import { createStore } from 'vuex';
import axios from 'axios';
import Cookies from 'js-cookie'; // Importez js-cookie

export default createStore({
  state: {
    authenticated: false,
    user: null,
    // AI Model persistence state
    modelIA: {
      modelContext: null,
      modelHistory: [],
      currentModel: null,
      modelSettings: {
        temperature: 0.7,
        maxTokens: 2000,
        topP: 1,
        frequencyPenalty: 0,
        presencePenalty: 0
      }
    }
  },
  mutations: {
    SET_AUTH(state, { authenticated, user }) {
      state.authenticated = authenticated;
      state.user = user;
    },
    LOGOUT(state) {
      state.authenticated = false;
      state.user = null;
    },
    // AI Model mutations
    SET_MODEL_CONTEXT(state, context) {
      state.modelIA.modelContext = context;
      localStorage.setItem('modelIAContext', JSON.stringify(context));
    },
    ADD_TO_HISTORY(state, interaction) {
      state.modelIA.modelHistory.push(interaction);
      if (state.modelIA.modelHistory.length > 50) {
        state.modelIA.modelHistory.shift();
      }
      localStorage.setItem('modelIAHistory', JSON.stringify(state.modelIA.modelHistory));
    },
    SET_CURRENT_MODEL(state, model) {
      state.modelIA.currentModel = model;
      localStorage.setItem('currentModel', model);
    },
    UPDATE_MODEL_SETTINGS(state, settings) {
      state.modelIA.modelSettings = { ...state.modelIA.modelSettings, ...settings };
      localStorage.setItem('modelSettings', JSON.stringify(state.modelIA.modelSettings));
    },
    CLEAR_MODEL_DATA(state) {
      state.modelIA = {
        modelContext: null,
        modelHistory: [],
        currentModel: null,
        modelSettings: {
          temperature: 0.7,
          maxTokens: 2000,
          topP: 1,
          frequencyPenalty: 0,
          presencePenalty: 0
        }
      };
      localStorage.removeItem('modelIAContext');
      localStorage.removeItem('modelIAHistory');
      localStorage.removeItem('currentModel');
      localStorage.removeItem('modelSettings');
    }
  },
  actions: {
    login({ commit }, user) {
      commit('SET_AUTH', { authenticated: true, user });
    },
    logout({ commit }) {
      Cookies.remove('token'); // Supprime le token du cookie
      commit('LOGOUT');
    },
    checkUserStatus({ commit }) {
      const token = Cookies.get('token'); // Récupère le token depuis les cookies
      if (token) {
        try {
          // Décoder le token JWT
          const base64Url = token.split('.')[1];
          const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
          const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
          }).join(''));
          const userData = JSON.parse(jsonPayload);

          // Vérifiez si le rôle est présent dans le token
          const userRole = userData['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'User';

          commit('SET_AUTH', { 
            authenticated: true, 
            user: {
              Id:userData.Id,
              email: userData.email,
              role: userRole
            }
          });
        } catch (error) {
          console.error('Error decoding token:', error);
          Cookies.remove('token'); // Supprime le token du cookie en cas d'erreur
          commit('LOGOUT');
        }
      } else {
        commit('SET_AUTH', { authenticated: false, user: null });
      }
    },
    async fetchUser({ commit }) {
      try {
        const token = Cookies.get('token'); // Récupère le token depuis les cookies
        const response = await axios.get("/api/profile", {
          headers: { Authorization: `Bearer ${token}` }
        });
        // Utilise SET_AUTH pour mettre à jour l'état
        commit("SET_AUTH", { 
          authenticated: true, 
          user: {
            ...response.data.data, // Inclut toutes les données de l'API
            role: response.data.data.role || 'User', // Ajoute le rôle si nécessaire
            Id:response.data.data.Id
          }
        });
      } catch (error) {
        console.error("Erreur lors du chargement de l'utilisateur :", error);
        throw error;
      }
    },
    // AI Model actions
    initializeModelFromStorage({ commit }) {
      const context = localStorage.getItem('modelIAContext');
      const history = localStorage.getItem('modelIAHistory');
      const model = localStorage.getItem('currentModel');
      const settings = localStorage.getItem('modelSettings');

      if (context) commit('SET_MODEL_CONTEXT', JSON.parse(context));
      if (history) commit('ADD_TO_HISTORY', JSON.parse(history));
      if (model) commit('SET_CURRENT_MODEL', model);
      if (settings) commit('UPDATE_MODEL_SETTINGS', JSON.parse(settings));
    },
    setModelContext({ commit }, context) {
      commit('SET_MODEL_CONTEXT', context);
    },
    addToHistory({ commit }, interaction) {
      commit('ADD_TO_HISTORY', interaction);
    },
    setCurrentModel({ commit }, model) {
      commit('SET_CURRENT_MODEL', model);
    },
    updateModelSettings({ commit }, settings) {
      commit('UPDATE_MODEL_SETTINGS', settings);
    },
    clearModelData({ commit }) {
      commit('CLEAR_MODEL_DATA');
    }
  },
  getters: {
    isAuthenticated(state) {
      return state.authenticated;
    },
    user(state) {
      return state.user;
    },
    // AI Model getters
    getModelContext: state => state.modelIA.modelContext,
    getModelHistory: state => state.modelIA.modelHistory,
    getCurrentModel: state => state.modelIA.currentModel,
    getModelSettings: state => state.modelIA.modelSettings
  }
});