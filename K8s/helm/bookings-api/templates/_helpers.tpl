{{- define "bookings-api.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "bookings-api.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- $name := default .Chart.Name .Values.nameOverride -}}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}

{{- define "bookings-api.labels" -}}
app.kubernetes.io/name: {{ include "bookings-api.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
helm.sh/chart: {{ printf "%s-%s" .Chart.Name .Chart.Version | quote }}
{{- end -}}

{{- define "bookings-api.selectorLabels" -}}
app.kubernetes.io/name: {{ include "bookings-api.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

{{- define "bookings-api.serviceAccountName" -}}
{{- if .Values.serviceAccount.create -}}
{{- if .Values.serviceAccount.name -}}
{{- .Values.serviceAccount.name -}}
{{- else -}}
{{- include "bookings-api.fullname" . -}}
{{- end -}}
{{- else -}}
{{- default "default" .Values.serviceAccount.name -}}
{{- end -}}
{{- end -}}

{{- define "bookings-api.previewServiceName" -}}
{{- printf "%s-preview" (include "bookings-api.fullname" .) | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "bookings-api.postgresHost" -}}
{{- if .Values.postgres.enabled -}}
{{- printf "%s-postgres" (include "bookings-api.fullname" .) -}}
{{- else -}}
{{- default "postgresql" .Values.postgres.host -}}
{{- end -}}
{{- end -}}

{{- define "bookings-api.redisHost" -}}
{{- if .Values.redis.enabled -}}
{{- printf "%s-redis" (include "bookings-api.fullname" .) -}}
{{- else -}}
{{- default "redis" .Values.redis.host -}}
{{- end -}}
{{- end -}}

{{- define "bookings-api.resources" -}}
{{- $preset := .Values.resourcesPreset | default "" -}}
{{- if eq $preset "small" -}}
requests:
  cpu: 100m
  memory: 128Mi
  ephemeral-storage: 128Mi
limits:
  cpu: 250m
  memory: 256Mi
  ephemeral-storage: 512Mi
{{- else if eq $preset "medium" -}}
requests:
  cpu: 200m
  memory: 256Mi
  ephemeral-storage: 256Mi
limits:
  cpu: 500m
  memory: 512Mi
  ephemeral-storage: 1Gi
{{- else if eq $preset "large" -}}
requests:
  cpu: 500m
  memory: 512Mi
  ephemeral-storage: 512Mi
limits:
  cpu: "1"
  memory: 1Gi
  ephemeral-storage: 2Gi
{{- else -}}
{{- toYaml .Values.resources -}}
{{- end -}}
{{- end -}}
