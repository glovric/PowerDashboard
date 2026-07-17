import { onMounted, onBeforeUnmount, ref, shallowRef } from 'vue'
import { useRouter } from 'vue-router';
import geoData from '@/assets/countries.geo.json'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { isoMap, getCountryColor } from "./EuroMapUtils";

export function useEuroMap() {

    const map = shallowRef(null)
    const geojsonLayer = shallowRef(null)
    const mapContainer = ref(null)
    const router = useRouter();

    onMounted(() => {
        map.value = L.map(mapContainer.value).setView([51.526, 15.2551], 4)

        L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
            subdomains: 'abcd',
            maxZoom: 10
        }).addTo(map.value);

        try {
            const geojson = geoData;
            const europeISO3 = Object.keys(isoMap);
            const europeanCountries = geojson.features.filter(f => europeISO3.includes(f.id));

            geojsonLayer.value = L.geoJson(
                { type: 'FeatureCollection', features: europeanCountries },
                {
                    style: feature => ({
                        fillColor: getCountryColor(feature.id),
                        weight: 1.5,
                        color: '#fff',
                        fillOpacity: 0.7
                    }),

                    onEachFeature: (feature, layer) => {
                        const countryIso = isoMap[feature.id]
                        const countryName = feature.properties.name || 'Country'

                        const btnBase = "text-decoration:none;color:white;padding:6px 10px;border-radius:4px;font-size:12px;font-weight:600;display:inline-block;margin:2px;transition:opacity 0.2s;"
                        
                        const popupContent = `
                        <div style="text-align:center;min-width:180px;font-family:sans-serif;">
                            <h4 style="margin:0 0 10px 0;color:#333;border-bottom:1px solid #eee;padding-bottom:5px;">${countryName}</h4>
                            <div style="display:flex;gap:5px;justify-content:center;flex-wrap:wrap;">
                            <a href="#" data-route="/current?selectedCountry=${countryIso}" style="${btnBase}background:#007bff;" onmouseover="this.style.opacity='0.8'" onmouseout="this.style.opacity='1'">Latest</a>
                            <a href="#" data-route="/history?selectedCountry=${countryIso}" style="${btnBase}background:#6c757d;" onmouseover="this.style.opacity='0.8'" onmouseout="this.style.opacity='1'">History</a>
                            <a href="#" data-route="/forecast?selectedCountry=${countryIso}" style="${btnBase}background:#28a745;" onmouseover="this.style.opacity='0.8'" onmouseout="this.style.opacity='1'">Forecast</a>
                            </div>
                        </div>
                        `

                        layer.bindPopup(popupContent);

                        layer.on({
                            mouseover: e => {
                                e.target.setStyle({
                                weight: 3,
                                color: '#666',
                                fillOpacity: 0.9
                                });
                            },
                            mouseout: e => {
                                geojsonLayer.value.resetStyle(e.target);
                            },
                            click: e => {
                                L.DomEvent.stopPropagation(e);
                                layer.openPopup();
                                const countryCenter = layer.getBounds().getCenter();
                                const mapInstance = map.value;
                                mapInstance.flyTo(countryCenter, 5, { duration: 0.6 });
                            },
                            popupopen: e => {
                                const popupEl = e.popup.getElement();

                                popupEl.addEventListener('click', (event) => {
                                    const link = event.target.closest('[data-route]');
                                    if (!link) return;
                                    event.preventDefault();
                                    const route = link.dataset.route;
                                    router.push(route);
                                });
                            }
                        });
                    }
                })
            .addTo(map.value);

        } 
        catch (err) {
            console.error('GeoJSON load failed:', err)
        }
    })

    onBeforeUnmount(() => {
        map.value?.remove()
    })

    return mapContainer;

}