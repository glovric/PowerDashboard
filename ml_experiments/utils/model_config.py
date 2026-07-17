from dataclasses import dataclass

@dataclass
class XGBConfig:
    max_depth: int
    learning_rate: float
    subsample: float
    colsample_bytree: float
    min_child_weight: int
    gamma: float
    reg_lambda: float
    reg_alpha: float
    n_estimators: int = 3000
    early_stopping_rounds: int = 50
    booster: str = 'gbtree'
    objective: str = 'reg:squarederror'
    tree_method: str = 'hist'
    eval_metric: str = 'rmse'
    
    def keys(self):
        return self.__dataclass_fields__.keys()
    
    def __getitem__(self, key):
        return getattr(self, key)

XGB_NOWCAST_60 = XGBConfig(
    max_depth=6,
    learning_rate=0.030026871579509014,
    subsample=0.8,
    colsample_bytree=0.95,
    min_child_weight=7,
    gamma=4.5,
    reg_lambda=5.0,
    reg_alpha=4.5
)

XGB_NOWCAST_15 = XGBConfig(
    max_depth=7,
    learning_rate=0.0582,
    subsample=0.85,
    colsample_bytree=1.0,
    min_child_weight=5,
    gamma=2.5,
    reg_lambda=1.5,
    reg_alpha=3.0
)

XGB_FORECAST_60_6 = XGBConfig(
    max_depth=8,
    learning_rate=0.030334399419411587,
    subsample=0.5,
    colsample_bytree=0.75,
    min_child_weight=7,
    gamma=1.5,
    reg_lambda=9.5,
    reg_alpha=9.5
)

XGB_FORECAST_60_12 = XGBConfig(
    max_depth=9,
    learning_rate=0.034826583070147306,
    subsample=0.75,
    colsample_bytree=0.9,
    min_child_weight=6,
    gamma=0.0,
    reg_lambda=1.5,
    reg_alpha=5.0
)

XGB_FORECAST_60_24 = XGBConfig(
    max_depth=7,
    learning_rate=0.052832107399551013,
    subsample=0.6,
    colsample_bytree=0.9,
    min_child_weight=6,
    gamma=4.5,
    reg_lambda=3.0,
    reg_alpha=7.0
)

XGB_FORECAST_15_6 = XGBConfig(
    max_depth=10,
    learning_rate=0.036856028596795806,
    subsample=0.75,
    colsample_bytree=0.65,
    min_child_weight=10,
    gamma=5.0,
    reg_lambda=3.0,
    reg_alpha=2.0
)

XGB_FORECAST_15_12 = XGBConfig(
    max_depth=9,
    learning_rate=0.03426755461028722,
    subsample=0.6,
    colsample_bytree=0.9,
    min_child_weight=8,
    gamma=2.5,
    reg_lambda=6.5,
    reg_alpha=3.5
)

XGB_FORECAST_15_24 = XGBConfig(
    max_depth=7,
    learning_rate=0.03977365940017587,
    subsample=0.95,
    colsample_bytree=0.9,
    min_child_weight=8,
    gamma=3.0,
    reg_lambda=5.5,
    reg_alpha=3.5
)

NOWCAST_CONFIG = {
    15: XGB_NOWCAST_15,
    60: XGB_NOWCAST_60
}

FORECAST_CONFIG = {
    15: {
        6: XGB_FORECAST_15_6,
        12: XGB_FORECAST_15_12,
        24: XGB_FORECAST_15_24    
    },
    60: {
        6: XGB_FORECAST_60_6,
        12: XGB_FORECAST_60_12,
        24: XGB_FORECAST_60_24
    }
}