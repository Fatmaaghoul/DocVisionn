<template>
  <div class="profile-page">
    <!-- Sidebar Profile Card -->
    <div class="profile-sidebar">
      <div class="profile-card">
        <div class="avatar">{{ userInitials }}</div>
        <h3>{{ user.userName }}</h3>

        <ul class="profile-info-list">
          <li><i class="bi bi-envelope"></i> {{ user.email }}</li>
          <li><i class="bi bi-telephone"></i> {{ user.phoneNumber }}</li>
          <!--<li><i class="bi bi-geo-alt"></i> {{ user.city || 'Ville inconnue' }}</li>-->
        </ul>
      </div>
    </div>
    <!-- Main Profile Form -->
    <div class="profile-main">
      <div class="profile-form-card">
        <div class="form-header">
          <h4>changer mon profil</h4>
          <button v-if="!isEditing" class="edit-btn" @click="isEditing = true">
            <i class="bi bi-pencil"></i> modifier
          </button>
        </div>
        <form @submit.prevent="submitForm">
<!-- Name -->
<label>Nom</label>
<div v-if="!isEditing" class="profile-readonly">{{ user.userName }}</div>
<input v-else v-model="editUser.userName" type="text" required />

<!-- Email -->
<label>Email</label>
<div v-if="!isEditing" class="profile-readonly">{{ user.email }}</div>
<input v-else v-model="editUser.email" type="email" disabled />

<!-- Phone -->
<label>Téléphone</label>
<div v-if="!isEditing" class="profile-readonly">{{ user.phoneNumber }}</div>
<input v-else v-model="editUser.phoneNumber" type="tel" disabled/>

<!-- Passwords -->
<hr />
<h5>Changer de mot de passe</h5>
<label>mot de passe actuel</label>
<div v-if="!isEditing" class="profile-readonly">••••••••</div>
<div v-else class="password-group">
  <input v-model="editUser.currentPassword" :type="showCurrentPassword ? 'text' : 'password'" />
  <button type="button" class="eye-btn" @click="showCurrentPassword = !showCurrentPassword" tabindex="0">
    <i :class="showCurrentPassword ? 'bi bi-eye-slash' : 'bi bi-eye'" />
  </button>
</div>
<label>neauveau mot de passe</label>
<div v-if="!isEditing" class="profile-readonly">••••••••</div>
<div v-else class="password-group">
  <input v-model="editUser.newPassword" :type="showNewPassword ? 'text' : 'password'" />
  <button type="button" class="eye-btn" @click="showNewPassword = !showNewPassword" tabindex="0">
    <i :class="showNewPassword ? 'bi bi-eye-slash' : 'bi bi-eye'" />
  </button>
</div>
          <div v-if="errorMessage" class="alert-error">{{ errorMessage }}</div>
          <div v-if="successMessage" class="alert-success">{{ successMessage }}</div>
          <button v-if="isEditing" type="submit" class="save-btn">Enregistrer</button>
          <button v-if="isEditing" type="button" class="cancel-btn" @click="cancelEdit">Annuler</button>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { ref, onMounted, computed } from 'vue';
import Cookies from 'js-cookie';

export default {
  name: "Profile",
  setup() {
    const user = ref({});
    const editUser = ref({
      userName: "",
      email: "",
      phoneNumber: "",
      currentPassword: "",
      newPassword: ""
    });
    const errorMessage = ref("");
    const successMessage = ref("");
    const showCurrentPassword = ref(false);
    const showNewPassword = ref(false);
    const isEditing = ref(false);

    // Initiales pour l'avatar
    const userInitials = computed(() => {
      if (!user.value.userName) return "?";
      return user.value.userName.split(' ').map(n => n[0]).join('').toUpperCase();
    });

    onMounted(async () => {
      try {
        const token = Cookies.get('token');
        const response = await axios.get("/api/profile", {
          headers: { Authorization: `Bearer ${token}` }
        });
        user.value = response.data.data;
        editUser.value = { ...response.data.data, currentPassword: "", newPassword: "" };
      } catch (error) {
        errorMessage.value = "Erreur lors du chargement du profil.";
      }
    });

    const submitForm = async () => {
  try {
    const token = Cookies.get('token');
    const response = await axios.put("/api/profile", editUser.value, {
      headers: { Authorization: `Bearer ${token}` }
    });
    successMessage.value = "Profil mis à jour avec succès !";
    errorMessage.value = "";
    
    // Recharger les données du profil
    const profileResponse = await axios.get("/api/profile", {
      headers: { Authorization: `Bearer ${token}` }
    });
    user.value = profileResponse.data.data;
    editUser.value = { ...profileResponse.data.data, currentPassword: "", newPassword: "" };
    
    isEditing.value = false;
    setTimeout(() => successMessage.value = "", 3000);
  } catch (error) {
    errorMessage.value = error.response?.data?.message || "Erreur lors de la mise à jour du profil.";
  }
};

    const cancelEdit = () => {
      isEditing.value = false;
      editUser.value = { ...user.value, currentPassword: editUser.value.currentPassword, newPassword: editUser.value.newPassword };
      errorMessage.value = "";
      successMessage.value = "";
      showCurrentPassword.value = false;
      showNewPassword.value = false;
    };

    return {
      user,
      editUser,
      errorMessage,
      successMessage,
      userInitials,
      showCurrentPassword,
      showNewPassword,
      isEditing,
      submitForm,
      cancelEdit
    };
  },
};
</script>

<style scoped>
.profile-page {
  display: flex;
  gap: 2rem;
  justify-content: center;
  align-items: flex-start;
  padding: 8rem;
  background: #fafbfc;
  min-height: 100vh;
}
.profile-sidebar {
  width: 280px;
}
.profile-card {
  background: #fff;
  border-radius: 10px;
  padding: 2rem 1.5rem;
  box-shadow: 0 2px 8px #0001;
  text-align: center;
}
.avatar {
  width: 70px;
  height: 70px;
  background: #635bff;
  color: #fff;
  border-radius: 50%;
  font-size: 2.2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
}
.profile-info-list {
  list-style: none;
  padding: 0;
  margin: 1rem 0 0 0;
  color: #555;
  font-size: 0.98rem;
}
.profile-info-list li {
  margin: 0.5rem 0;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.profile-main {
  flex: 1;
  max-width: 420px;
}
.profile-form-card {
  background: #fff;
  border-radius: 10px;
  padding: 2rem 2rem 1.5rem 2rem;
  box-shadow: 0 2px 8px #0001;
}
.profile-form-card h4 {
  margin-bottom: 1.5rem;
}
.profile-form-card label {
  display: block;
  margin-top: 1rem;
  margin-bottom: 0.3rem;
  font-weight: 500;
}
.profile-form-card input {
  width: 100%;
  padding: 0.7rem;
  border: 1px solid #e0e0e0;
  border-radius: 6px;
  margin-bottom: 0.5rem;
  font-size: 1rem;
}
.save-btn {
  margin-top: 1.2rem;
  width: 100%;
  background: #635bff;
  color: #fff;
  border: none;
  border-radius: 6px;
  padding: 0.8rem;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}
.save-btn:hover {
  background: #4b3eea;
}
.alert-error {
  background: #ffeaea;
  color: #c23030;
  border-radius: 6px;
  padding: 0.7rem 1rem;
  margin: 0.7rem 0;
  font-size: 0.98rem;
}
.alert-success {
  background: #e6f7ee;
  color: #0d8050;
  border-radius: 6px;
  padding: 0.7rem 1rem;
  margin: 0.7rem 0;
  font-size: 0.98rem;
}
.password-group {
  display: flex;
  align-items: center;
  position: relative;
}
.password-group input {
  flex: 1;
  margin-bottom: 0;
}
.profile-readonly {
  padding: 0.7rem 0.9rem;
  background: #f7f7fa;
  border-radius: 6px;
  color: #444;
  margin-bottom: 0.5rem;
  font-size: 1rem;
  min-height: 42px;
  display: flex;
  align-items: center;
}
.eye-btn {
  background: none;
  border: none;
  color: #888;
  font-size: 1.2rem;
  margin-left: -2.2rem;
  cursor: pointer;
  z-index: 2;
  padding: 0 0.5rem;
  height: 100%;
  display: flex;
  align-items: center;
}
.eye-btn:disabled {
  color: #ccc;
  cursor: not-allowed;
}
.form-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}
.edit-btn {
  background: #f5f5f5;
  border: none;
  color: #635bff;
  border-radius: 6px;
  padding: 0.4rem 1rem;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
  display: flex;
  align-items: center;
  gap: 0.4rem;
}
.edit-btn:hover {
  background: #ecebff;
}
.cancel-btn {
  margin-top: 0.7rem;
  width: 100%;
  background: #f5f5f5;
  color: #635bff;
  border: none;
  border-radius: 6px;
  padding: 0.8rem;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.2s;
}
.cancel-btn:hover {
  background: #ecebff;
}
@media (max-width: 900px) {
  .profile-page {
    flex-direction: column;
    align-items: stretch;
  }
  .profile-sidebar {
    width: 100%;
    margin-bottom: 2rem;
  }
  .profile-main {
    max-width: 100%;
  }
}
</style>
