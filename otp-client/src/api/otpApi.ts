const BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000/api/v1/otp";

export type GenerateOtpResponse = { otp: string; expiresAtUtc: string; remainingSeconds: number };
export type ValidateOtpResponse = { isValid: boolean; status: string; message: string };

type ProblemDetails = { title?: string; detail?: string; errors?: Record<string, string[]> };

async function request<T>(path: string, init: RequestInit): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...init,
  });
  const body = await res.json().catch(() => ({}));

  if (!res.ok) {
    const problem = body as ProblemDetails;
    const firstError = problem.errors ? Object.values(problem.errors).flat()[0] : undefined;
    throw new Error(firstError ?? problem.detail ?? problem.title ?? `Request failed (${res.status})`);
  }
  return body as T;
}

export const generateOtp = (userId: string, dateTime: Date) =>
  request<GenerateOtpResponse>("/generate", {
    method: "POST",
    body: JSON.stringify({ userId, dateTime: dateTime.toISOString() }), // always send UTC
  });

export const validateOtp = (userId: string, otp: string) =>
  request<ValidateOtpResponse>("/validate", {
    method: "POST",
    body: JSON.stringify({ userId, otp }),
  });