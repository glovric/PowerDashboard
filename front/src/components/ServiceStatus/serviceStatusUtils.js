import { powerApi, inferenceApi, authApi } from "@/api";

export const loadServiceStatus = async () => {

    const [resPower, resInference, resAuth] = await Promise.all([
        powerApi.health(),
        inferenceApi.health(),
        authApi.health(),
      ]);

    return { power: resPower.success, inference: resInference.success, auth: resAuth.success }
}