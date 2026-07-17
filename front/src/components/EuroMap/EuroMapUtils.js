import { countries } from "@/utils/dashboardUtils";

export const isoMap = Object.fromEntries(
  countries.map(({ value, iso3 }) => [iso3, value])
);

export const countryColors = [
    '#FF6B6B', '#4ECDC4', '#45B7D1', '#96CEB4', '#FFEEAD', 
    '#D4A5A5', '#9B59B6', '#3498DB', '#E67E22', '#2ECC71',
    '#F1C40F', '#E74C3C', '#1ABC9C', '#34495E', '#95A5A6',
    '#FD79A8', '#00B894', '#0984E3', '#6C5CE7', '#A29BFE',
    '#FFEAA7', '#DFE6E9', '#FAB1A0', '#74B9FF', '#55E6C1'
]

export const getCountryColor = (id) => {
    if (!id) return '#CCCCCC'
    let hash = 0
    for (let i = 0; i < id.length; i++) {
        hash = id.charCodeAt(i) + ((hash << 5) - hash)
    }
    return countryColors[Math.abs(hash) % countryColors.length]
}