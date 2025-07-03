import { AuthServices } from '../../api/authService'
import { useState } from 'react';
import PropTypes from 'prop-types';

export const OtpVerificationForm = ({ onSubmit, isLoading, phoneNumberI}) => {
  const [otp, setOtp] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    const request = {
      phoneNumber: phoneNumberI,
      otpCode: otp
    }
    const response = await AuthServices.ValidateOtp(request)
    if(response && response.data == true){
      onSubmit(otp);
      return;
    }
    console.log(response);
    window.alert("otp no valido, revisa tu bandeja de entrada.");
  };

  return (
    <>
      <div className="text-center">
        <h1 className="text-2xl font-bold">Verificar tu número</h1>
        <p className="text-gray-500">Ingresa el código de 6 dígitos que te enviamos.</p>
      </div>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label htmlFor="otp" className="sr-only">Código OTP</label>
          <input id="otp" name="otp" type="text" maxLength={6} required
            className="w-full text-center tracking-[1em] text-2xl font-bold px-3 py-2 mt-1 border rounded-md"
            onChange={(e) => setOtp(e.target.value)} value={otp} placeholder="------"
          />
        </div>
        <button type="submit" disabled={isLoading || otp.length !== 6}
          className="w-full py-2 px-4 text-white bg-indigo-600 rounded-md hover:bg-indigo-700 disabled:bg-indigo-300">
          {isLoading ? 'Verificando...' : 'Continuar'}
        </button>
      </form>
    </>
  );
};

OtpVerificationForm.propTypes = {
  onSubmit: PropTypes.func.isRequired,
  isLoading: PropTypes.bool,
};