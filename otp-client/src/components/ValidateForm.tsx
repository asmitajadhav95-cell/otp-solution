import { useState, type FormEvent } from "react";
import { validateOtp, type ValidateOtpResponse } from "../api/otpApi";

type Props = {
  userId: string;
  onValidated: (result: ValidateOtpResponse) => void;
  onError: (message: string) => void;
};

export function ValidateForm({ userId, onValidated, onError }: Props) {
  const [code, setCode] = useState("");
  const [busy, setBusy] = useState(false);

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setBusy(true);
    try {
      onValidated(await validateOtp(userId.trim(), code));
      setCode("");
    } catch (err) {
      onError(err instanceof Error ? err.message : "Something went wrong");
    } finally {
      setBusy(false);
    }
  }

  // Not disabled after expiry on purpose: the SERVER is the judge of expiry.
  return (
    <form onSubmit={handleSubmit} className="card">
      <label>
        Enter password
        <input value={code} onChange={(e) => setCode(e.target.value.replace(/\D/g, ""))}
               inputMode="numeric" maxLength={6} pattern="\d{6}" required />
      </label>
      <button type="submit" disabled={busy}>{busy ? "Checking…" : "Validate"}</button>
    </form>
  );
}