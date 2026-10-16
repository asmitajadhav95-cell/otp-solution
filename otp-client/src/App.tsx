import { useState } from "react";
import type { GenerateOtpResponse, ValidateOtpResponse } from "./api/otpApi";
import { GenerateForm } from "./components/GenerateForm";
import { OtpDisplay } from "./components/OtpDisplay";
import { ValidateForm } from "./components/ValidateForm";
import { useCountdown } from "./hooks/useCountdown";

type Status = { ok: boolean; text: string } | null;

export default function App() {
  const [userId, setUserId] = useState("");
  const [otp, setOtp] = useState<GenerateOtpResponse | null>(null);
  const [deadline, setDeadline] = useState<number | null>(null);
  const [status, setStatus] = useState<Status>(null);
  const remaining = useCountdown(deadline);

  function handleGenerated(result: GenerateOtpResponse) {
    setOtp(result);
    // Use the server's remainingSeconds, counted on the browser's clock.
    setDeadline(Date.now() + result.remainingSeconds * 1000);
    setStatus(null);
  }

  function handleValidated(result: ValidateOtpResponse) {
    setStatus({ ok: result.isValid, text: result.message });
    if (result.isValid) {
      setOtp(null);            // used — hide it
      setDeadline(null);
    } else if (result.status !== "Invalid") {
      setDeadline(Date.now()); // expired / locked / not found → stop countdown
    }
  }

  const handleError = (message: string) => setStatus({ ok: false, text: message });

  return (
    <main>
      <h1>One-Time Password</h1>

      <GenerateForm userId={userId} onUserIdChange={setUserId}
                    onGenerated={handleGenerated} onError={handleError} />

      {otp && <OtpDisplay code={otp.otp} remaining={remaining} />}

      {otp && <ValidateForm userId={userId} onValidated={handleValidated} onError={handleError} />}

      {status && <p className={status.ok ? "msg ok" : "msg error"}>{status.text}</p>}
    </main>
  );
}