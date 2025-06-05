<template>
  <div class="admin-profile-container">
    <!-- Header with navigation -->
    <div class="header">
      <h1>Profil d'administrateur</h1>
      <nav aria-label="breadcrumb">
        <ol class="breadcrumb">
          <li class="breadcrumb-item active">Profil</li>
        </ol>
      </nav>
    </div>

    <!-- Profile section -->
    <div class="card profile-header-card">
      <div class="card-body">
        <div class="profile-info">
          <!-- Avatar avec la première lettre de l'email -->
          <div class="avatar-circle">
            {{ user?.email?.charAt(0).toUpperCase() }}
          </div>
          <div>
            <h2>{{ user?.email }}</h2>
            <p>{{ formattedRoles }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Personal Information -->
    <div class="card profile-info-card">
      <div class="card-body">
        <div class="section-header">
          <h3>Informations personnelles</h3>
          <button class="btn-edit" @click="toggleEditMode">
            <i class="bi bi-pencil"></i>{{ isEditing ? 'Annuler' : 'changer mot de passe' }}
          </button>
        </div>

        <div class="form-grid">
          <div class="form-group">
            <label>Nom</label>
            <input 
              v-model="editUser.userName" 
              :disabled="true"
              :class="isEditing ? 'form-control' : 'form-control-plaintext'" 
            />
          </div>
        
          <div class="form-group">
            <label>Email</label>
            <input 
              v-model="editUser.email" 
              
              :disabled="true"
              :class="isEditing ? 'form-control disabled' : 'form-control-plaintext'" 
            />
          </div>
          
          <div class="form-group">
            <label>Téléphone</label>
            <input 
              v-model="editUser.phoneNumber" 
              :disabled="true" 
              :class="isEditing ? 'form-control' : 'form-control-plaintext'" 
            />
          </div>

          <!-- Mode Édition : Ajout des champs de mot de passe -->
          <div v-if="isEditing" class="form-group full-width">
            <label>Mot de passe actuel</label>
            <div class="input-group">
              <input 
                v-model="editUser.currentPassword" 
                :type="showCurrentPassword ? 'text' : 'password'" 
                class="form-control" 
                required 
              />
              <button 
                type="button" 
                class="btn-toggle-password" 
                @click="toggleShowCurrentPassword"
              >
                <i :class="showCurrentPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
              </button>
            </div>
          </div>

          <div v-if="isEditing" class="form-group full-width">
            <label>Nouveau mot de passe</label>
            <div class="input-group">
              <input 
                v-model="editUser.newPassword" 
                :type="showNewPassword ? 'text' : 'password'" 
                class="form-control" 
              />
              <button 
                type="button" 
                class="btn-toggle-password" 
                @click="toggleShowNewPassword"
              >
                <i :class="showNewPassword ? 'bi bi-eye-slash' : 'bi bi-eye'"></i>
              </button>
            </div>
          </div>

          <!-- Bouton de sauvegarde en mode édition -->
          <div v-if="isEditing" class="form-group full-width">
            <button class="btn-save" @click="saveProfile">
              <i class="bi bi-save"></i>Enregistrer
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { computed, ref, onMounted } from 'vue';
import { useStore } from 'vuex';
import axios from 'axios';
import Cookies from 'js-cookie';

export default {
  name: 'AdminProfile',
  setup() {
    const store = useStore();
    const user = computed(() => store.state.user);
    const isEditing = ref(false);
    const showCurrentPassword = ref(false);
    const showNewPassword = ref(false);

    // Données éditables
    const editUser = ref({
      userName: '',
      email: '',
      phoneNumber: '',
      currentPassword: '',
      newPassword: ''
    });

    // Formater les rôles pour l'affichage
    const formattedRoles = computed(() => {
      return user.value?.roles?.join(', ') || '';
    });

    // Basculer entre le mode édition et consultation
    const toggleEditMode = () => {
      isEditing.value = !isEditing.value;
      if (!isEditing.value) {
        // Réinitialiser editUser avec les données actuelles de l'utilisateur
        editUser.value = {
          userName: user.value?.userName || '',
          email: user.value?.email || '',
          phoneNumber: user.value?.phoneNumber || '',
          currentPassword: '',
          newPassword: ''
        };
      }
    };

    // Basculer l'affichage du mot de passe actuel
    const toggleShowCurrentPassword = () => {
      showCurrentPassword.value = !showCurrentPassword.value;
    };

    // Basculer l'affichage du nouveau mot de passe
    const toggleShowNewPassword = () => {
      showNewPassword.value = !showNewPassword.value;
    };

    // Sauvegarder les modifications
    const saveProfile = async () => {
      try {
        const token = Cookies.get('token');
        const response = await axios.put("/api/profile", editUser.value, {
          headers: { Authorization: `Bearer ${token}` }
        });

        // Mettre à jour les données dans le store
        store.commit('SET_USER', response.data.data);
        isEditing.value = false;
        alert('Profile updated successfully!');
      } catch (error) {
        console.error("Error updating profile:", error);
        alert(error.response?.data?.message || "Failed to update profile.");
      }
    };

    // Charger les données utilisateur au montage du composant
    onMounted(async () => {
      try {
        const token = Cookies.get('token');
        const response = await axios.get("/api/profile", {
          headers: { Authorization: `Bearer ${token}` }
        });

        // Mettre à jour les données dans le store
        store.commit('SET_USER', response.data.data);

        // Initialiser editUser avec les données de l'utilisateur
        editUser.value = {
          userName: response.data.data.userName || '',
          email: response.data.data.email || '',
          phoneNumber: response.data.data.phoneNumber || '',
          currentPassword: '',
          newPassword: ''
        };
      } catch (error) {
        console.error("Error loading profile:", error);
        alert("Failed to load profile data.");
      }
    });

    return {
      user,
      editUser,
      isEditing,
      showCurrentPassword,
      showNewPassword,
      formattedRoles,
      toggleEditMode,
      toggleShowCurrentPassword,
      toggleShowNewPassword,
      saveProfile
    };
  }
};
</script>

<style scoped>
.admin-profile-container {
  padding: 1.5rem;
  max-width: 800px;
  margin: 0 auto;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.header h1 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #333;
}

.breadcrumb {
  background: transparent;
  padding: 0;
  margin: 0;
}

.breadcrumb-item {
  font-size: 0.875rem;
  color: #666;
}

.card {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  margin-bottom: 1.5rem;
  box-shadow: none;
}

.profile-header-card {
  background: #fff;
}

.profile-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.avatar-circle {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: #007bff;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  font-weight: 500;
}

.profile-info h2 {
  font-size: 1.25rem;
  font-weight: 500;
  margin: 0;
  color: #333;
}

.profile-info p {
  font-size: 0.875rem;
  color: #666;
  margin: 0;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.section-header h3 {
  font-size: 1.125rem;
  font-weight: 500;
  color: #333;
}

.btn-edit {
  background: none;
  border: 1px solid #007bff;
  color: #007bff;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  font-size: 0.875rem;
  cursor: pointer;
  transition: background-color 0.2s;
}

.btn-edit:hover {
  background: #e6f0ff;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-group.full-width {
  grid-column: 1 / -1;
}

.form-group label {
  font-size: 0.875rem;
  color: #666;
  margin-bottom: 0.25rem;
}

.form-control {
  padding: 0.5rem;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 1rem;
  color: #333;
}

.form-control:disabled {
  background: #f5f5f5;
  color: #999;
}

.form-control-plaintext {
  padding: 0.5rem 0;
  font-size: 1rem;
  color: #333;
  border: none;
  background: transparent;
}

.input-group {
  display: flex;
  align-items: center;
}

.btn-toggle-password {
  background: #fff;
  border: 1px solid #ccc;
  border-left: none;
  padding: 0.5rem;
  border-radius: 0 4px 4px 0;
  cursor: pointer;
}

.btn-toggle-password:hover {
  background: #f5f5f5;
}

.btn-save {
  background: #007bff;
  color: #fff;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  transition: background-color 0.2s;
}

.btn-save:hover {
  background: #0056b3;
}

@media (max-width: 768px) {
  .admin-profile-container {
    padding: 1rem;
  }

  .form-grid {
    grid-template-columns: 1fr;
  }

  .avatar-circle {
    width: 40px;
    height: 40px;
    font-size: 1rem;
  }
}
</style>