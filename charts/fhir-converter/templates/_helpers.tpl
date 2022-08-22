{{/* vim: set filetype=mustache: */}}
{{/*
Expand the name of the chart.
*/}}
{{- define "fhir.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
*/}}
{{- define "fhir.fullname" -}}
{{- $name := default .Chart.Name .Values.nameOverride -}}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "fhir.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/* vim: set filetype=mustache: */}}

{{/*
Construct the `labels.app` for used by all resources in this chart.
*/}}
{{- define "fhir.labels.app" -}}
{{- .Values.nameOverride | default .Chart.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Construct the `labels.chart` for used by all resources in this chart.
*/}}
{{- define "fhir.labels.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "ingress.annotations" -}}
{{- if (eq "gcp" .Values.ingress.cloud) -}}
kubernetes.io/ingress.class: nginx
cert-manager.io/issuer: letsencrypt-prod
appgw.ingress.kubernetes.io/ssl-redirect: "true"
nginx.ingress.kubernetes.io/affinity: cookie
nginx.ingress.kubernetes.io/session-cookie-name: route
nginx.ingress.kubernetes.io/session-cookie-hash: sha1
nginx.ingress.kubernetes.io/proxy-body-size: 51m
nginx.ingress.kubernetes.io/whitelist-source-range: {{ .Values.ingress.whitelist }}
{{- else if (eq "azure" .Values.ingress.cloud) -}}
kubernetes.io/ingress.class: azure/application-gateway
appgw.ingress.kubernetes.io/ssl-redirect: "true"
appgw.ingress.kubernetes.io/cookie-based-affinity: "true"
cert-manager.io/issuer: letsencrypt-prod
{{- end }}
{{- end }}

{{/*
Construct a set of secret environment variables to be mounted in pods.
*/}}
{{- define "fhir.mapenvsecrets" -}}
{{- /* ------------------------------ */ -}}
{{- /* ---------- API KEYS ---------- */ -}}
{{- /* ------------------------------ */ -}}
{{- if .Values.fhir.converterApiSecret }}
- name: CONVERSION_API_KEYS
  valueFrom:
    secretKeyRef:
      name: {{ .Values.fhir.converterApiSecret }}
      key: {{ .Values.fhir.converterApiSecretKey }}
{{- end }}
{{- end }}
