import { useState } from 'react';
import PropTypes from 'prop-types';
import { toast } from 'react-hot-toast';
import { PaymentServices } from '../../api/paymentService.js';

export const AddPaymentMethodForm = ({ onSuccess, onCancel }) => {
    const [isLoading, setIsLoading] = useState(false);
    const [type, setType] = useState('0');
    const [last4, setLast4] = useState(''); 

    const getPaymentTokenFromGateway = async (last4Digits) => {
        console.log("Simulando comunicación con el proveedor de pagos...");
        await new Promise(res => setTimeout(res, 1000));
        
        return {
            token: 'tok_' + Math.random().toString(36).substring(2),
            maskedIdentifier: `•••• ${last4Digits}`, 
            provider: 'Visa',
        };
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setIsLoading(true);
        toast.dismiss();

        try {
            const gatewayResponse = await getPaymentTokenFromGateway(last4);
            
            const methodData = {
                Type: parseInt(type, 10), 
                Provider: gatewayResponse.provider,
                Token: gatewayResponse.token,
                MaskedIdentifier: gatewayResponse.maskedIdentifier,
            };

            const response = await PaymentServices.addPaymentMethod(methodData);
            
            toast.success('¡Método de pago añadido con éxito!');
            onSuccess(response.data);

        } catch (error) {
            toast.error(error.response?.data?.message || 'No se pudo añadir el método de pago.');
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <h2 className="text-xl font-bold text-center">Añadir Nuevo Método de Pago</h2>
            
            <div>
                <label htmlFor="paymentMethodType" className="text-sm font-medium text-gray-700">Tipo de Método</label>
                <select 
                    id="paymentMethodType" 
                    value={type} 
                    onChange={(e) => setType(e.target.value)}
                    className="w-full px-3 py-2 mt-1 border border-gray-300 rounded-md"
                >
                    <option value="0">Tarjeta de Crédito</option>
                    <option value="1">Tarjeta de Débito</option>
                    <option value="2">Cuenta Bancaria</option>
                </select>
            </div>

            <div>
                <label className="text-sm font-medium text-gray-700">Datos de la Tarjeta</label>
                <div className="w-full p-3 mt-1 border border-gray-300 rounded-md bg-gray-50 text-gray-500">
                    Aquí iría el iFrame seguro del proveedor de pagos
                </div>
            </div>

            <div>
                <label htmlFor="last4" className="text-sm font-medium text-gray-700">Últimos 4 dígitos</label>
                <input
                    id="last4"
                    type="text"
                    maxLength="4"
                    value={last4}
                    onChange={(e) => setLast4(e.target.value)}
                    required
                    className="w-full px-3 py-2 mt-1 border border-gray-300 rounded-md"
                    placeholder="4242"
                />
            </div>

            <div className="flex justify-end space-x-3 pt-4">
                <button type="button" onClick={onCancel} className="px-4 py-2 text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200">
                    Cancelar
                </button>
                <button type="submit" disabled={isLoading} className="px-4 py-2 text-white bg-indigo-600 rounded-md hover:bg-indigo-700 disabled:bg-indigo-300">
                    {isLoading ? 'Guardando...' : 'Guardar Método'}
                </button>
            </div>
        </form>
    );
};

AddPaymentMethodForm.propTypes = {
    onSuccess: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
};