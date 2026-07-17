<template>
  <div class="settings-wrapper" ref="wrapperRef">
    
    <button 
      class="settings-trigger" 
      :class="{ active: isOpen }"
      @click="toggleMenu"
      aria-label="Dashboard Settings"
      title="Dashboard Settings"
    >
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="12" cy="12" r="3"></circle>
        <path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z"></path>
      </svg>
    </button>

    <transition name="fade-slide">
      <div v-if="isOpen" class="settings-dropdown">
        <div class="dropdown-header">
          <span>View Options</span>
        </div>
        
        <div class="dropdown-content">
          <!-- Group 1: Edit Mode Toggle -->
          <label class="dropdown-item toggle-row">
            <span class="item-label">Edit Layout</span>
            <div class="mini-toggle">
              <input type="checkbox" :checked="editLayout" @click="toggleEditLayout" :disabled="tabularView"/>
              <span class="slider"></span>
            </div>
          </label>

          <!-- Divider -->
          <div class="divider"></div>

          <!-- Group 2: View Mode Segmented Control -->
          <div class="dropdown-item">
            <span class="item-label">View Mode</span>
            <div class="view-toggle-segmented">
              <!-- Tile -->
              <button 
                :class="['segment-btn', { active: chartViewMode === 'tile' }]" 
                @click="setChartViewMode('tile')"
                :disabled="tabularView"
              >
                <svg viewBox="0 0 24 24" fill="currentColor">
                  <rect x="3" y="3" width="8" height="8" rx="1.5"/>
                  <rect x="13" y="3" width="8" height="8" rx="1.5"/>
                  <rect x="3" y="13" width="8" height="8" rx="1.5"/>
                  <rect x="13" y="13" width="8" height="8" rx="1.5"/>
                </svg>
              </button>

              <!-- List -->
              <button 
                :class="['segment-btn', { active: chartViewMode === 'list' }]" 
                @click="setChartViewMode('list')"
                :disabled="tabularView"
              >
                <svg viewBox="0 0 24 24" fill="currentColor">
                  <rect x="3" y="4" width="18" height="3" rx="1.5"/>
                  <rect x="3" y="10.5" width="18" height="3" rx="1.5"/>
                  <rect x="3" y="17" width="18" height="3" rx="1.5"/>
                </svg>
              </button>
            </div>
          </div>

          <div class="divider"></div>

          <label class="dropdown-item toggle-row">
            <span class="item-label">Tabular view</span>
            <div class="mini-toggle">
              <input type="checkbox" :checked="tabularView" @click="toggleTabularView" />
              <span class="slider"></span>
            </div>
          </label>

        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
    import { defineModel } from 'vue';
    import { useDashboardSettings } from '@/components/DashboardSettings/useDashboardSettings'
    import { useDashboardViewMode } from '@/composables/useDashboardViewMode';

    const { chartViewMode, tabularView, setChartViewMode, toggleTabularView } = useDashboardViewMode();
    const { isOpen, wrapperRef, toggleMenu } = useDashboardSettings();

    const editLayout = defineModel();
    const toggleEditLayout = () => {
        editLayout.value = !editLayout.value;
    }
</script>

<style lang="scss" src="@/styles/DashboardSettings.scss" scoped></style>