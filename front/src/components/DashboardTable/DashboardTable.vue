<template>

  <div class="table-wrapper">

    <div class="table-header-actions">

      <div class="control-group">
        <label>Export</label>
        <select class="dropdown" v-model="format">
          <option value="csv">CSV</option>
          <option value="xlsx">XLSX</option>
        </select>
      </div>
      
      <button class="btn-download" @click="handleExport">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
          <polyline points="7 10 12 15 17 10"></polyline>
          <line x1="12" y1="15" x2="12" y2="3"></line>
        </svg>
      </button>

    </div>

    <table v-if="tabularData">
      <thead>
        <tr>
          <th v-for="header in tabularData.headers" :key="header">
            {{ header }}
          </th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(row, rIndex) in tabularData.rows" :key="rIndex">
          <td v-for="(colKey, colIndex) in tabularData.columnKeys" :key="colIndex">
             {{ row[colKey] }}
          </td>
        </tr>
      </tbody>
    </table>
  </div>

</template>

<script setup>
  import { defineModel } from 'vue';
  import { useDashboardTable } from "./useDashboardTable";

  const tabularData = defineModel();
  const { format, handleExport } = useDashboardTable(tabularData);
</script>
<style lang="scss" src="@/styles/TableActions.scss" scoped></style>
<style lang="scss" src="@/styles/DashboardCommon.scss" scoped></style>
