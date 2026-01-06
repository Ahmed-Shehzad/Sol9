{{- define "sol9.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "sol9.chart" -}}
{{- printf "%s-%s" .Chart.Name (.Chart.Version | replace "+" "_") -}}
{{- end -}}

{{- define "sol9.fullname" -}}
{{- printf "%s-%s" (include "sol9.name" .root) .component | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "sol9.labels" -}}
app.kubernetes.io/name: {{ include "sol9.name" .root }}
helm.sh/chart: {{ include "sol9.chart" .root }}
app.kubernetes.io/instance: {{ .root.Release.Name }}
app.kubernetes.io/managed-by: {{ .root.Release.Service }}
app.kubernetes.io/component: {{ .component }}
{{- end -}}

{{- define "sol9.selectorLabels" -}}
app.kubernetes.io/name: {{ include "sol9.name" .root }}
app.kubernetes.io/instance: {{ .root.Release.Name }}
app.kubernetes.io/component: {{ .component }}
{{- end -}}

{{- define "sol9.secretsName" -}}
{{ printf "%s-secrets" (include "sol9.name" .) }}
{{- end -}}

{{- define "sol9.postgresHost" -}}
{{- .Values.postgres.host -}}
{{- end -}}

{{- define "sol9.redisConnectionString" -}}
{{- if .Values.redis.connectionString -}}
{{- .Values.redis.connectionString -}}
{{- else -}}
{{- printf "redis://%s:%v" .Values.redis.host .Values.redis.port -}}
{{- end -}}
{{- end -}}
