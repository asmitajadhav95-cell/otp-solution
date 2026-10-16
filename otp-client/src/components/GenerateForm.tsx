import { useState, type FormEvent } from "react";
import { generateOtp, type GenerateOtpResponse } from "../api/otpApi";

const toLocalInput = (d: Date) =>
  new Date(d.getTime() - d.getTimezoneOffset() * 60000).toISOString().slice(0, 19);

type Props = {
  userId: string;
  onUserIdChange: (value: string) => void;
  onGenerated: (result: GenerateOtpResponse) => void;
  onError: (message: string) => void;
};

export function GenerateForm({ userId, onUserIdChange, onGenerated, onError }: Props) {
  const [useNow, setUseNow] = useState(true);
  const [dateTime, setDateTime] = useState(() => toLocalInput(new Date()));
  const [busy, setBusy] = useState(false);

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setBusy(true);
    try {
      const when = useNow ? new Date() : new Date(dateTime);
      onGenerated(await generateOtp(userId.trim(), when));
    } catch (err) {
      onError(err instanceof Error ? err.message : "Something went wrong");
    } finally {
      setBusy(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="card">
      <label>
        User Id
        <input value={userId} onChange={(e) => onUserIdChange(e.target.value)} maxLength={50} required />
      </label>

      <label className="inline">
        <input type="checkbox" checked={useNow} onChange={(e) => setUseNow(e.target.checked)} />
        Use current time
      </label>

      {!useNow && (
        <label>
          Date &amp; Time
          <input type="datetime-local" step="1" value={dateTime}
                 onChange={(e) => setDateTime(e.target.value)} required />
        </label>
      )}

      <button type="submit" disabled={busy}>{busy ? "Generating…" : "Generate password"}</button>
    </form>
  );
}