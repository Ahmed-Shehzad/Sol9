{{- define "gateway-api.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "gateway-api.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- $name := default .Chart.Name .Values.nameOverride -}}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}

{{- define "gateway-api.labels" -}}
app.kubernetes.io/name: {{ include "gateway-api.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
helm.sh/chart: {{ printf "%s-%s" .Chart.Name .Chart.Version | quote }}
{{- end -}}

{{- define "gateway-api.selectorLabels" -}}
app.kubernetes.io/name: {{ include "gateway-api.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

{{- define "gateway-api.serviceAccountName" -}}
{{- if .Values.serviceAccount.create -}}
{{- if .Values.serviceAccount.name -}}
{{- .Values.serviceAccount.name -}}
{{- else -}}
{{- include "gateway-api.fullname" . -}}
{{- end -}}
{{- else -}}
{{- default "default" .Values.serviceAccount.name -}}
{{- end -}}
{{- end -}}

{{- define "gateway-api.previewServiceName" -}}
{{- printf "%s-preview" (include "gateway-api.fullname" .) | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "gateway-api.metricsServiceName" -}}
{{- if .Values.metrics.serviceName -}}
{{- .Values.metrics.serviceName -}}
{{- else -}}
{{- include "gateway-api.fullname" . -}}
{{- end -}}
{{- end -}}

{{- define "gateway-api.analysisTemplateName" -}}
{{- if .Values.rollout.analysis.templateName -}}
{{- .Values.rollout.analysis.templateName -}}
{{- else -}}
{{- printf "%s-success-rate" (include "gateway-api.fullname" .) -}}
{{- end -}}
{{- end -}}

{{- define "gateway-api.bookingsAddress" -}}
{{- if .Values.env.bookingsAddress -}}
{{- .Values.env.bookingsAddress -}}
{{- else -}}
{{- if .Values.tls.enabled -}}
{{- printf "https://%s-bookings-api:80" .Release.Name -}}
{{- else -}}
{{- printf "http://%s-bookings-api:80" .Release.Name -}}
{{- end -}}
{{- end -}}
{{- end -}}

{{- define "gateway-api.ordersAddress" -}}
{{- if .Values.env.ordersAddress -}}
{{- .Values.env.ordersAddress -}}
{{- else -}}
{{- if .Values.tls.enabled -}}
{{- printf "https://%s-orders-api:80" .Release.Name -}}
{{- else -}}
{{- printf "http://%s-orders-api:80" .Release.Name -}}
{{- end -}}
{{- end -}}
{{- end -}}

{{- define "gateway-api.resources" -}}
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
