<template>
  <div class="consult-tab">
    <div class="document-preview">
      <iframe 
        v-if="document.fileUrl"
        :src="document.fileUrl"
        frameborder="0"
        width="100%"
        height="600px"
      ></iframe>
      <div v-else class="no-preview">
        <i class="bi bi-file-earmark-text"></i>
        <p>Aucun aperçu disponible</p>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    document: {
      type: Object,
      required: true
    }
  }
};
</script>

<style scoped>
.document-preview {
  height: 600px;
  border: 1px solid #eee;
  border-radius: 5px;
  overflow: hidden;
}

.no-preview {
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #666;
}

.no-preview i {
  font-size: 3rem;
  margin-bottom: 15px;
}
</style>