<template>
  <div class="euromap-wrapper">
    <div class="header">
      <h1>Select Country</h1>
    </div>
    
    <div class="map-container">
      <div id="map" ref="mapContainer"></div>
    </div>
  </div>
</template>

<script setup>
  import { useEuroMap } from './useEuroMap';
  const mapContainer = useEuroMap();
</script>

<style lang="scss" src="@/styles/EuroMap.scss" scoped></style>
